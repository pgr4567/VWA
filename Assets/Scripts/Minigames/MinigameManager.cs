using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public abstract class MinigameManager : MonoBehaviour {
    [HideInInspector]
    public string gameID = string.Empty;
    public int number = -1;
    public string worldID = string.Empty;
    [SerializeField] protected Minigame minigame;
    [SerializeField] protected bool addPlayersToRandomTeam = true;
    [SerializeField] protected List<Transform> teamSpawnPositions = new List<Transform> ();
    protected List<string> players = new List<string> ();
    protected Dictionary<int, MinigameTeam> teams = new Dictionary<int, MinigameTeam> ();
    protected string lobbyOwner = string.Empty;
    protected bool isRunning;
    private Queue<Transform> unusedTeamSpawnPositions = new Queue<Transform> ();
    protected void Update () {
        if (isRunning && GameManager.instance.isServer) {
            GameLoopServer ();
        } else if (isRunning) {
            GameLoopClient ();
        }
    }
    public void JoinPlayer (string username, bool isOwner, string name) {
        if (players.Count >= minigame.maxPlayersPerTeam * minigame.maxTeams) {
            //TODO: sth, lobby is full
            return;
        }
        players.Add (username);
        if (addPlayersToRandomTeam) {
            bool addedToTeam = false;
            foreach (MinigameTeam team in teams.Values) {
                if (team.players.Count < minigame.requiredPlayersPerTeam) {
                    team.players.Add (username);
                    addedToTeam = true;
                    break;
                }
            }
            if (!addedToTeam) {
                foreach (MinigameTeam team in teams.Values) {
                    if (team.players.Count < minigame.maxPlayersPerTeam) {
                        team.players.Add (username);
                        addedToTeam = true;
                        break;
                    }
                }
            }
            if (!addedToTeam) {
                if (teams.Count < minigame.maxTeams) {
                    foreach (int c in Colors.colors) {
                        if (!teams.ContainsKey (c)) {
                            teams.Add (c, new MinigameTeam ());
                            break;
                        }
                    }
                    foreach (MinigameTeam team in teams.Values) {
                        if (team.players.Count < minigame.requiredPlayersPerTeam) {
                            team.players.Add (username);
                            addedToTeam = true;
                            break;
                        }
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

            MainNetworkManager.instance.Players[username].Send (new MirrorMinigameMessage () { name = name, number = number });
            MainNetworkManager.instance.Players[username].Send (new FreezeMovementMessage () { freeze = true });
            GameManager.instance.TargetShowLobbyCanvas (MainNetworkManager.instance.Players[username], minigame, teams[Colors.colors[0]], teams[Colors.colors[1]], isOwner, gameID);

            foreach (string uName in players) {
                GameManager.instance.TargetUpdateTeams (MainNetworkManager.instance.Players[uName], teams[Colors.colors[0]], teams[Colors.colors[1]]);
            }
        }
    }
    public void LeavePlayer (PlayerMovement player, string username) {
        if ((players.Count - 1 < minigame.requiredPlayersPerTeam * minigame.requiredTeams && isRunning) || (players.Count - 1 == 0 && !isRunning)) {
            Debug.Log ("Player: " + username + " left the minigame. There are now too few players. Closing minigame.");
            MinigameDispatcher.instance.RemoveMinigameLobby (minigame.name, number, gameID);
        } else {
            players.Remove (username);
            teams.Where (t => t.Value.players.Contains (username)).First ().Value.players.Remove (username);

            if (lobbyOwner == username) {
                lobbyOwner = players.First ();
                if (!isRunning) {
                    GameManager.instance.TargetUpdateIsOwner (MainNetworkManager.instance.Players[username], true);
                }
            }

            foreach (string uName in players) {
                GameManager.instance.TargetUpdateTeams (MainNetworkManager.instance.Players[uName], teams[Colors.colors[0]], teams[Colors.colors[1]]);
            }

            Debug.Log ("Player: " + username + " left the minigame.");
            LeavePlayerInternal (player, username);
        }
    }
    public async virtual void StartGameServer () {
        //TODO: REMOVE -1 LOL
        if (!isRunning && players.Count >= minigame.requiredPlayersPerTeam * minigame.requiredTeams - 1) {
            isRunning = true;
            foreach (MinigameTeam team in teams.Values) {
                foreach (string pName in team.players) {
                    GameObject player = MainNetworkManager.instance.PlayerObjs[pName];
                    player.GetComponent<NetworkTransform> ().ServerTeleport (team.spawnPoint);
                    await System.Threading.Tasks.Task.Delay (100);
                    MainNetworkManager.instance.Players[pName].Send (new FreezeMovementMessage () { freeze = false });
                    GameManager.instance.TargetHideLobbyCanvas (MainNetworkManager.instance.Players[pName]);
                }
            }
        } else {
            Debug.LogWarning ("either not enough players or already running. Cannot Start Minigame");
            //TODO: Error
        }
    }
    public void TeleportAllPlayers (Vector3 position) {
        foreach (string username in players) {
            TeleportPlayer (MainNetworkManager.instance.PlayerObjs[username].GetComponent<PlayerMovement> (), position);
        }
    }
    public void LeaveAllPlayers () {
        foreach (string username in players) {
            LeavePlayerInternal (MainNetworkManager.instance.PlayerObjs[username].GetComponent<PlayerMovement> (), username);
        }
    }
    public void ResetGame () {
        players.Clear ();
        teams.Clear ();
        isRunning = false;

        teamSpawnPositions.ForEach (t => unusedTeamSpawnPositions.Enqueue (t));

        for (int i = 0; i < minigame.requiredTeams; i++) {
            foreach (int c in Colors.colors) {
                if (!teams.ContainsKey (c)) {
                    teams.Add (c, new MinigameTeam () { spawnPoint = unusedTeamSpawnPositions.Dequeue ().position });
                    break;
                }
            }
        }
    }
    private async void TeleportPlayer (PlayerMovement player, Vector3 position) {
        player.netIdentity.connectionToClient.Send (new FreezeMovementMessage () { freeze = true });
        player.GetComponent<NetworkTransform> ().ServerTeleport (position);
        await System.Threading.Tasks.Task.Delay (100);
        player.netIdentity.connectionToClient.Send (new FreezeMovementMessage () { freeze = false });
    }
    private void LeavePlayerInternal (PlayerMovement player, string username) {
        MainNetworkManager.instance.Players[username].Send (new FreezeMovementMessage () { freeze = false });
        GameManager.instance.TargetHideLobbyCanvas (MainNetworkManager.instance.Players[username]);
        TeleportPlayer (player, GameManager.instance.worldSpawn);
        NetworkServer.SendToClientOfPlayer (MainNetworkManager.instance.Players[username].identity, new MirrorRemoveMinigameInstanceMessage () { worldID = number, gameID = gameID, name = minigame.name });

    }
    public void ScoreTeam (int points, int color) {
        if (GameManager.instance.isServer) {
            teams[color].points += points;
        }
    }
    protected abstract void GameLoopServer ();
    protected virtual void GameLoopClient () {

    }
    public virtual void StartGameClient () {
        isRunning = true;
    }
}