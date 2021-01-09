using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using General;
using Mirror;
using Networking;
using Player;
using UnityEngine;

namespace Minigames {
    public abstract class MinigameManager : MonoBehaviour {
        [HideInInspector] public string gameID = string.Empty;

        public int number = -1;
        public string worldID = string.Empty;
        [SerializeField] protected Minigame minigame;
        [SerializeField] protected bool addPlayersToRandomTeam = true;
        [SerializeField] protected List<Transform> teamSpawnPositions = new List<Transform> ();
        private readonly Queue<Transform> _unusedTeamSpawnPositions = new Queue<Transform> ();
        protected bool isRunning;
        protected string lobbyOwner = string.Empty;
        protected List<string> players = new List<string> ();
        protected Dictionary<int, MinigameTeam> teams = new Dictionary<int, MinigameTeam> ();

        protected void Update () {
            if (isRunning && GameManager.instance.isServer) {
                GameLoopServer ();
            } else if (isRunning) {
                GameLoopClient ();
            }
        }

        public void JoinPlayer (string username, bool isOwner) {
            if (players.Count >= minigame.maxPlayersPerTeam * minigame.maxTeams) //TODO: sth, lobby is full
            {
                return;
            }

            players.Add (username);
            if (!addPlayersToRandomTeam) {
                return;
            }

            bool addedToTeam = false;
            foreach (MinigameTeam team in teams.Values.Where (team =>
                team.players.Count < minigame.requiredPlayersPerTeam)) {
                team.players.Add (username);
                addedToTeam = true;
                break;
            }

            if (!addedToTeam) {
                foreach (MinigameTeam team in teams.Values.Where (team =>
                    team.players.Count < minigame.maxPlayersPerTeam)) {
                    team.players.Add (username);
                    addedToTeam = true;
                    break;
                }
            }

            if (!addedToTeam) {
                if (teams.Count < minigame.maxTeams) {
                    foreach (int c in Colors.colors.Where (c => !teams.ContainsKey (c))) {
                        teams.Add (c, new MinigameTeam ());
                        break;
                    }

                    foreach (MinigameTeam team in teams.Values.Where (team =>
                        team.players.Count < minigame.requiredPlayersPerTeam)) {
                        team.players.Add (username);
                        addedToTeam = true;
                        break;
                    }

                    if (!addedToTeam) {
                        //TODO: this is weird, it should not happen
                        Debug.LogWarning ("Weird activity, should not have been triggered.");
                        return;
                    }
                }
            }

            if (isOwner) {
                lobbyOwner = username;
            }

            MainNetworkManager.instance.players[username].Send (new MirrorMinigameMessage
                { name = minigame.name, number = number });
            MainNetworkManager.instance.players[username].Send (new FreezeMovementMessage { freeze = true });
            GameManager.instance.TargetShowLobbyCanvas (MainNetworkManager.instance.players[username], minigame,
                teams[Colors.colors[0]], teams[Colors.colors[1]], isOwner, gameID);
            GameManager.instance.TargetSetInGame (MainNetworkManager.instance.players[username], true);
            foreach (string uName in players) {
                GameManager.instance.TargetUpdateTeams (MainNetworkManager.instance.players[uName],
                    teams[Colors.colors[0]], teams[Colors.colors[1]]);
            }
        }

        public void LeavePlayer (PlayerMovement player, string username) {
            if (players.Count - 1 < minigame.requiredPlayersPerTeam * minigame.requiredTeams && isRunning ||
                players.Count - 1 == 0 && !isRunning) {
                Debug.Log ("Player: " + username +
                           " left the minigame. There are now too few players. Closing minigame.");
                MinigameDispatcher.instance.RemoveMinigameLobby (minigame.name, number, gameID);
            } else {
                players.Remove (username);
                teams.First (t => t.Value.players.Contains (username)).Value.players.Remove (username);

                if (lobbyOwner == username) {
                    lobbyOwner = players.First ();
                    if (!isRunning) {
                        GameManager.instance.TargetUpdateIsOwner (MainNetworkManager.instance.players[lobbyOwner],
                            true);
                    }
                }

                foreach (string uName in players) {
                    GameManager.instance.TargetUpdateTeams (MainNetworkManager.instance.players[uName],
                        teams[Colors.colors[0]], teams[Colors.colors[1]]);
                }

                Debug.Log ("Player: " + username + " left the minigame.");
                LeavePlayerInternal (player, username);
            }
        }

        public virtual async void StartGameServer () {
            //TODO: REMOVE -1 LOL
            if (!isRunning && players.Count >= minigame.requiredPlayersPerTeam * minigame.requiredTeams - 1) {
                isRunning = true;
                foreach (MinigameTeam team in teams.Values)
                foreach (string pName in team.players) {
                    //MainNetworkManager.instance.playerObjs[pName].GetComponent<PlayerMovement> ().RpcSetVisible (false);
                    GameObject player = MainNetworkManager.instance.playerObjs[pName];
                    player.GetComponent<NetworkTransform> ().ServerTeleport (team.spawnPoint);
                    await Task.Delay (100);
                    MainNetworkManager.instance.players[pName].Send (new FreezeMovementMessage { freeze = false });
                    GameManager.instance.TargetHideLobbyCanvas (MainNetworkManager.instance.players[pName]);
                }
            } else {
                Debug.LogWarning ("either not enough players or already running. Cannot Start Minigame");
                //TODO: Error
            }
        }

        public void TeleportAllPlayers (Vector3 position) {
            foreach (string username in players) {
                TeleportPlayer (MainNetworkManager.instance.playerObjs[username].GetComponent<PlayerMovement> (),
                    position);
            }
        }

        public void LeaveAllPlayers () {
            foreach (string username in players) {
                LeavePlayerInternal (MainNetworkManager.instance.playerObjs[username].GetComponent<PlayerMovement> (),
                    username);
            }
        }

        public void ResetGame () {
            players.Clear ();
            teams.Clear ();
            isRunning = false;

            teamSpawnPositions.ForEach (t => _unusedTeamSpawnPositions.Enqueue (t));

            for (int i = 0; i < minigame.requiredTeams; i++) {
                foreach (int c in Colors.colors.Where (c => !teams.ContainsKey (c))) {
                    teams.Add (c, new MinigameTeam { spawnPoint = _unusedTeamSpawnPositions.Dequeue ().position });
                    break;
                }
            }
        }

        private async void TeleportPlayer (PlayerMovement player, Vector3 position) {
            player.netIdentity.connectionToClient.Send (new FreezeMovementMessage { freeze = true });
            player.GetComponent<NetworkTransform> ().ServerTeleport (position);
            await Task.Delay (100);
            player.netIdentity.connectionToClient.Send (new FreezeMovementMessage { freeze = false });
        }

        private void LeavePlayerInternal (PlayerMovement player, string username) {
            GameManager.instance.TargetSetInGame (MainNetworkManager.instance.players[username], false);
            Debug.Log (MainNetworkManager.instance.playerObjs[username]);
            player.RpcSetVisible (true);
            MainNetworkManager.instance.players[username].Send (new FreezeMovementMessage { freeze = false });
            GameManager.instance.TargetHideLobbyCanvas (MainNetworkManager.instance.players[username]);
            TeleportPlayer (player, GameManager.instance.worldSpawn);
            NetworkServer.SendToClientOfPlayer (MainNetworkManager.instance.players[username].identity,
                new MirrorRemoveMinigameInstanceMessage { worldID = number, gameID = gameID, name = minigame.name });
        }

        private void ShowEndScreen (MinigameTeam teamWon) {
            foreach (string player in players) {
                GameManager.instance.TargetShowFinishGameCanvas (MainNetworkManager.instance.players[player], minigame,
                    teamWon);
            }
        }

        public void ScoreTeam (int points, int color) {
            if (GameManager.instance.isServer) {
                teams[color].points += points;
            }
        }

        protected void FinishGame (MinigameTeam teamWon) {
            isRunning = false;
            ShowEndScreen (teamWon);
            MinigameDispatcher.instance.RemoveMinigameLobby (minigame.name, number, gameID);
        }

        protected abstract void GameLoopServer ();

        protected virtual void GameLoopClient () { }

        public virtual void StartGameClient () { isRunning = true; }
    }
}