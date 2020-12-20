using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class MinigameDispatcher : NetworkBehaviour {
    public int worldSize { get; private set; } = 30;
    [SerializeField] private List<Minigame> minigames = new List<Minigame> ();
    [SerializeField] private Transform minigameStartPosition;
    [SerializeField] private int worldSpacer = 2;

    private Dictionary<string, int> minigameRow = new Dictionary<string, int> ();
    private readonly Dictionary<string, List<int>> minigameWorlds = new Dictionary<string, List<int>> ();
    private readonly Dictionary<string, MinigameManager> existingGames = new Dictionary<string, MinigameManager> ();

    public static MinigameDispatcher instance;

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
        string name = msg.name;
        string username = msg.username;

        Minigame game = minigames.Where (g => g.name == name).First ();

        (MinigameManager, int)result = CreateMinigameInstance (name);
        MinigameManager manager = result.Item1;
        int number = result.Item2;

        if (manager == null) {
            // World was not created.
            return;
        }
        existingGames.Add (manager.transform.position.ToString () + name, manager);
        manager.gameID = manager.transform.position.ToString () + name;
        manager.number = number;

        manager.ResetGame ();
        manager.JoinPlayer (username, true, name);
    }

    public void JoinMinigameLobby (string playerName, string gameID, string name) {
        GetMinigameManagerForGame (gameID).JoinPlayer (playerName, false, name);
    }
    public void LeaveMinigameLobby (string playerName, string gameID) {
        GetMinigameManagerForGame (gameID).LeavePlayer (MainNetworkManager.instance.PlayerObjs[playerName].GetComponent<PlayerMovement> (), playerName);
    }

    public void RemoveMinigameLobby (string name, int number, string gameID) {
        MinigameManager manager = GetMinigameManagerForGame (gameID);
        manager.LeaveAllPlayers ();

        minigameWorlds[name].Remove (number);
        existingGames.Remove (gameID);

        Destroy (manager.gameObject);
    }
    public void RemoveMinigameInstanceClient (string name, int number, string gameID) {
        minigameWorlds[name].Remove (number);
        MinigameManager manager = GetMinigameManagerForGame (gameID);
        existingGames.Remove (gameID);

        Destroy (manager.gameObject);
    }
    public MinigameManager CreateMinigameInstanceClient (string name, int number) {
        Minigame game = minigames.Where (g => g.name == name).First ();

        int row = minigameRow[game.name];
        if (!minigameWorlds.ContainsKey (game.name)) {
            minigameWorlds[game.name] = new List<int> ();
        }

        minigameWorlds[game.name].Add (number);
        Vector3 coordinates = new Vector3 (row * worldSize + worldSize / 2, 0, number * worldSize + worldSize / 2) + minigameStartPosition.position + new Vector3 (worldSpacer, 0, worldSpacer);

        GameObject go = Instantiate (game.world, coordinates, Quaternion.identity);
        MinigameManager manager = go.GetComponent<MinigameManager> ();
        existingGames.Add (manager.transform.position.ToString () + name, manager);
        manager.gameID = manager.transform.position.ToString () + name;
        manager.number = number;
        return manager;
    }
    public MinigameManager GetMinigameManagerForGame (string gameID) {
        if (existingGames.TryGetValue (gameID, out MinigameManager manager)) {
            return manager;
        }
        return null;
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
    private void JoinMinigame (JoinMinigameMessage msg) {
        JoinMinigameLobby (msg.username, msg.gameID, msg.name);
    }
    private void LeaveMinigame (LeaveMinigameMessage msg) {
        LeaveMinigameLobby (msg.username, msg.gameID);
    }
    private void PopulateMinigameRows () {
        for (int i = 0; i < minigames.Count; i++) {
            minigameRow.Add (minigames[i].name, i);
        }
    }
    // Wenn die Methode True zurück gibt, sind die coordinates die Koordinaten vom ersten freien Slot
    // für dieses Minispiel.
    private bool GetAvailableCoordinates (Minigame game, out Vector3 coordinates, out int number) {
        int row = minigameRow[game.name];
        if (!minigameWorlds.ContainsKey (game.name)) {
            minigameWorlds[game.name] = new List<int> ();
        }
        // Findet den ersten freien Slot, rechnet die Koordinaten aus und gibt sie zurück.
        List<int> instances = minigameWorlds[game.name];
        for (int i = 0; i < game.maxInstances; i++) {
            if (!instances.Contains (i)) {
                minigameWorlds[game.name].Add (i);
                coordinates = new Vector3 (row * worldSize + worldSize / 2, 0, i * worldSize + worldSize / 2) + minigameStartPosition.position + new Vector3 (worldSpacer, 0, worldSpacer);
                number = i;
                return true;
            }
        }
        coordinates = Vector3.zero;
        number = 0;
        return false;
    }
    private (MinigameManager, int)CreateMinigameInstance (string name) {
        Minigame game = minigames.Where (g => g.name == name).First ();
        if (GetAvailableCoordinates (game, out Vector3 coordinates, out int number)) {
            GameObject go = Instantiate (game.world, coordinates, Quaternion.identity);
            return (go.GetComponent<MinigameManager> (), number);
        } else {
            Debug.Log ("The maximum amount of parallel instances of this game has already been reached. Please wait until one game finishes.");
            return (null, 0);
        }
    }
}