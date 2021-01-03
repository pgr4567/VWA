using System.Collections.Generic;
using System.Linq;
using Mirror;
using Networking;
using Player;
using UnityEngine;

namespace Minigames {
    public class MinigameDispatcher : NetworkBehaviour {
        public static MinigameDispatcher instance;
        [SerializeField] private List<Minigame> minigames = new List<Minigame> ();
        [SerializeField] private Transform minigameStartPosition;
        [SerializeField] private int worldSpacer = 2;

        private readonly Dictionary<string, MinigameManager>
            _existingGames = new Dictionary<string, MinigameManager> ();

        private readonly Dictionary<string, int> _minigameRow = new Dictionary<string, int> ();
        private readonly Dictionary<string, List<int>> _minigameWorlds = new Dictionary<string, List<int>> ();
        public int worldSize { get; } = 30;

        private void Awake () {
            if (instance != null) {
                Debug.LogWarning ("There must only be one MinigameDispatcher in the scene.");
                Destroy (this);
            }

            instance = this;

            PopulateMinigameRows ();
            SetupEventHandlers ();
        }

        public void CreateMinigameLobby (CreateMinigameMessage msg) {
            string name     = msg.name;
            string username = msg.username;

            Minigame game = minigames.First (g => g.name == name);

            (MinigameManager, int) result  = CreateMinigameInstance (name);
            MinigameManager        manager = result.Item1;
            int                    number  = result.Item2;

            if (manager == null) // World was not created.
            {
                return;
            }

            Vector3 position = manager.transform.position;
            _existingGames.Add (position + name, manager);
            manager.gameID = position + name;
            manager.number = number;

            SendMinigameListUpdateToAll ();

            manager.ResetGame ();
            manager.JoinPlayer (username, true);
        }

        public void JoinMinigameLobby (string playerName, string gameID) {
            GetMinigameManagerForGame (gameID)?.JoinPlayer (playerName, false);
        }

        public void LeaveMinigameLobby (string playerName, string gameID) {
            GetMinigameManagerForGame (gameID)
                .LeavePlayer (MainNetworkManager.instance.playerObjs[playerName].GetComponent<PlayerMovement> (),
                    playerName);
        }

        public void RemoveMinigameLobby (string name, int number, string gameID) {
            MinigameManager manager = GetMinigameManagerForGame (gameID);
            manager.LeaveAllPlayers ();

            _minigameWorlds[name].Remove (number);
            _existingGames.Remove (gameID);

            SendMinigameListUpdateToAll ();

            Destroy (manager.gameObject);
        }

        public void RemoveMinigameInstanceClient (string name, int number, string gameID) {
            _minigameWorlds[name].Remove (number);
            MinigameManager manager = GetMinigameManagerForGame (gameID);
            _existingGames.Remove (gameID);

            Destroy (manager.gameObject);
        }

        public MinigameManager CreateMinigameInstanceClient (string name, int number) {
            Minigame game = minigames.First (g => g.name == name);

            int row = _minigameRow[game.name];
            if (!_minigameWorlds.ContainsKey (game.name)) {
                _minigameWorlds[game.name] = new List<int> ();
            }

            _minigameWorlds[game.name].Add (number);
            Vector3 coordinates = new Vector3 (row * worldSize + worldSize / 2, 0, number * worldSize + worldSize / 2) +
                                  minigameStartPosition.position + new Vector3 (worldSpacer, 0, worldSpacer);

            GameObject      go       = Instantiate (game.world, coordinates, Quaternion.identity);
            MinigameManager manager  = go.GetComponent<MinigameManager> ();
            Vector3         position = manager.transform.position;
            _existingGames.Add (position + name, manager);
            manager.gameID = position + name;
            manager.number = number;
            return manager;
        }

        public MinigameManager GetMinigameManagerForGame (string gameID) {
            return _existingGames.TryGetValue (gameID, out MinigameManager manager) ? manager : null;
        }

        private void SendMinigameListUpdateToAll () {
            NetworkServer.SendToAll (new MinigameListUpdateMessage { gameIDs = _existingGames.Keys.ToArray () });
        }

        public void SendMinigameListUpdateToClient (NetworkIdentity player) {
            NetworkServer.SendToClientOfPlayer (player,
                new MinigameListUpdateMessage { gameIDs = _existingGames.Keys.ToArray () });
        }

        private void SetupEventHandlers () {
            NetworkServer.RegisterHandler<CreateMinigameMessage> (CreateMinigameLobby);
            NetworkServer.RegisterHandler<StartMinigameMessage> (StartMinigame);
            NetworkServer.RegisterHandler<JoinMinigameMessage> (JoinMinigame);
            NetworkServer.RegisterHandler<LeaveMinigameMessage> (LeaveMinigame);
        }

        private void StartMinigame (StartMinigameMessage msg) {
            GetMinigameManagerForGame (msg.gameID).StartGameServer ();
            NetworkServer.SendToAll (msg);
        }

        private void JoinMinigame (JoinMinigameMessage msg) { JoinMinigameLobby (msg.username, msg.gameID); }

        private void LeaveMinigame (LeaveMinigameMessage msg) { LeaveMinigameLobby (msg.username, msg.gameID); }

        private void PopulateMinigameRows () {
            for (int i = 0; i < minigames.Count; i++) {
                _minigameRow.Add (minigames[i].name, i);
            }
        }

        // Wenn die Methode True zurück gibt, sind die coordinates die Koordinaten vom ersten freien Slot
        // für dieses Minispiel.
        private bool GetAvailableCoordinates (Minigame game, out Vector3 coordinates, out int number) {
            int row = _minigameRow[game.name];
            if (!_minigameWorlds.ContainsKey (game.name)) {
                _minigameWorlds[game.name] = new List<int> ();
            }

            // Findet den ersten freien Slot, rechnet die Koordinaten aus und gibt sie zurück.
            List<int> instances = _minigameWorlds[game.name];
            for (int i = 0; i < game.maxInstances; i++) {
                if (!instances.Contains (i)) {
                    _minigameWorlds[game.name].Add (i);
                    coordinates = new Vector3 (row * worldSize + worldSize / 2, 0, i * worldSize + worldSize / 2) +
                                  minigameStartPosition.position + new Vector3 (worldSpacer, 0, worldSpacer);
                    number = i;
                    return true;
                }
            }

            coordinates = Vector3.zero;
            number      = 0;
            return false;
        }

        private (MinigameManager, int) CreateMinigameInstance (string name) {
            Minigame game = minigames.First (g => g.name == name);
            if (GetAvailableCoordinates (game, out Vector3 coordinates, out int number)) {
                GameObject go = Instantiate (game.world, coordinates, Quaternion.identity);
                return (go.GetComponent<MinigameManager> (), number);
            }

            Debug.Log (
                "The maximum amount of parallel instances of this game has already been reached. Please wait until one game finishes.");
            return (null, 0);
        }
    }
}