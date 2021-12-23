using System;
using System.Collections.Generic;
using System.Linq;
using Chat;
using Minigames;
using Mirror;
using Networking;
using Networking.RequestMessages;
using Player;
using UI;
using UnityEngine;

namespace General {
    public class GameManager : NetworkBehaviour {
        public static GameManager instance;
        public bool DEBUG;
        [HideInInspector] public bool isInGUI;
        [HideInInspector] public new bool isServer;
        [SerializeField] private GameObject lobbyCanvas;
        [SerializeField] private GameObject finishGameCanvas;
        [SerializeField] private Transform worldSpawnTransform;
        public string currentGame { get; private set; } = "";
        public Action onMinigameListUpdate;
        public string username { get; private set; }
        public string sessionToken { get; private set; }
        public string[] minigameIDs { get; private set; }
        public bool isInGame;
        public int currentTeam = -1;
        public GameObject player;

        public Vector3 worldSpawn {
            get { return worldSpawnTransform.position; }
        }

        private void Awake () {
            if (instance != null) {
                Debug.LogWarning ("There should only be one GameManager in the scene!");
                Destroy (this);
            }

            instance = this;
        }

        private void Start () {
            if (!NetworkServer.active) {
                username = PlayerPrefs.GetString ("username");
                sessionToken = PlayerPrefs.GetString ("sessionToken");
                return;
            }

            isServer = true;
            GameObject go = new GameObject { name = "Overview Camera" };
            go.AddComponent<Camera> ();
            go.transform.position = new Vector3 (0, 20, 0);
            go.transform.eulerAngles = new Vector3 (90, 0, 0);

            Cursor.lockState = CursorLockMode.None;
        }

        public void UpdateMinigameList (string[] gameIDs) {
            minigameIDs = gameIDs;
            onMinigameListUpdate?.Invoke ();
        }

        public void JoinMinigame (string gameID) {
            if (isServer || currentGame != "") {
                return;
            }
            NetworkClient.Send (
                new JoinMinigameMessage { username = username, gameID = gameID });
            currentGame = gameID;
        }
        public void LeaveMinigame () {
            if (isServer || currentGame == "") {
                return;
            }
            NetworkClient.Send (new LeaveMinigameMessage { username = username, gameID = currentGame });
            currentGame = "";
        }

        [TargetRpc]
        public void TargetShowLobbyCanvas (NetworkConnection target, Minigame game, MinigameTeam team1,
            MinigameTeam team2,
            bool isOwner, string gameID) {
            lobbyCanvas.GetComponent<LobbyCanvas> ().ShowCanvas (game.name, game.description, game.stats, team1.players,
                team2.players, isOwner, gameID);
            lobbyCanvas.SetActive (true);
            MouseController.instance.ShowCursor ();
        }

        [TargetRpc]
        public void TargetUpdateIsOwner (NetworkConnection target, bool isOwner) {
            lobbyCanvas.GetComponent<LobbyCanvas> ().UpdateIsOwner (isOwner);
        }

        [TargetRpc]
        public void TargetUpdateTeams (NetworkConnection target, MinigameTeam team1, MinigameTeam team2) {
            lobbyCanvas.GetComponent<LobbyCanvas> ().UpdateTeams (team1.players, team2.players);
        }

        [TargetRpc]
        public void TargetHideLobbyCanvas (NetworkConnection target) {
            lobbyCanvas.SetActive (false);
            MouseController.instance.HideCursor ();
        }

        [TargetRpc]
        public void TargetShowFinishGameCanvas (NetworkConnection target, Minigame game, MinigameTeam teamWon) {
            finishGameCanvas.GetComponent<FinishGameCanvas> ().ShowCanvas (game, teamWon);
            finishGameCanvas.SetActive (true);
            isInGUI = true;
            MouseController.instance.ShowCursor ();
        }

        [TargetRpc]
        public void TargetSetInGame (NetworkConnection target, bool inGame) {
            isInGame = inGame;
        }

        [TargetRpc]
        public void TargetSetGameID (NetworkConnection target, string gameID) {
            currentGame = gameID;
        }

        [TargetRpc]
        public void TargetSetCurrentTeam (NetworkConnection target, int team) {
            currentTeam = team;
        }

        [TargetRpc]
        public void TargetChangePlayerSpeed (NetworkConnection target, float moveSpeed, float lookSpeed) {
            player.GetComponent<PlayerMovement> ().movementSpeed = moveSpeed;
            player.GetComponent<CameraLook> ().lookSpeed = lookSpeed;
        }

        [TargetRpc]
        public void TargetResetPlayerSpeed (NetworkConnection target) {
            player.GetComponent<PlayerMovement> ().movementSpeed = PlayerMovement.standardMovementSpeed;
            player.GetComponent<CameraLook> ().lookSpeed = CameraLook.standardLookSpeed;
        }

        [TargetRpc]
        public void TargetUpdateMoney (NetworkConnection target) {
            RequestManagerClient.instance.SendRequest ("money");
        }
    }
}