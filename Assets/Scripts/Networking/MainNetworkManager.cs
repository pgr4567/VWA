using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MainNetworkManager : NetworkManager {
    public Dictionary<string, NetworkConnection> Players = new Dictionary<string, NetworkConnection> ();
    public Dictionary<string, GameObject> PlayerObjs = new Dictionary<string, GameObject> ();
    public static MainNetworkManager instance;
    public GameObject localPlayer;
    public GameObject remotePlayer;

    public override void Awake () {
        if (instance != null) {
            Debug.Log ("There must only be one MainNetworkManager in the scene!");
            Destroy (this);
        }
        instance = this;
        base.Awake ();
    }
    public void AddPlayerToDictionary (NetworkConnection conn) {
        string username = ((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username;
        Debug.Log ($"Trying to connect: {username}");
        if (Players.ContainsKey (username)) {
            conn.Disconnect ();
            Debug.LogWarning ("Somebody who is already logged in, tried to log in.");
            return;
        }
        Debug.Log ($"Conn for: {username}: {conn}");
        Players.Add (username, conn);

        CreatePlayerObject (conn, username);
    }
    public void RemovePlayerFromDictionary (NetworkConnection conn) {
        string username = ((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username;
        Players.Remove (username);
        PlayerObjs.Remove (username);
    }
    public override void OnServerConnect (NetworkConnection conn) {
        AddPlayerToDictionary (conn);
    }
    public override void OnServerDisconnect (NetworkConnection conn) {
        RemovePlayerFromDictionary (conn);

        base.OnServerDisconnect (conn);
    }
    public override void OnClientDisconnect (NetworkConnection conn) {
        //TODO: TEMPORARY
        NetworkClient.Send (new LeaveMinigameMessage () { username = PlayerPrefs.GetString ("username"), gameID = "(27.0, 0.0, 7.0)Labyrinth" });

        base.OnClientDisconnect (conn);
    }

    public void CreatePlayerObject (NetworkConnection conn, string username) {
        Debug.Log ($"GO for: {username}: {conn}");
        GameObject go = Instantiate (localPlayer, transform.position, Quaternion.identity);
        PlayerObjs.Add (username, go);

        NetworkServer.AddPlayerForConnection (conn, go);
    }
}