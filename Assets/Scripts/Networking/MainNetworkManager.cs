using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MainNetworkManager : NetworkManager {
    public Dictionary<string, NetworkConnection> Players = new Dictionary<string, NetworkConnection> ();
    public static MainNetworkManager instance;
    public override void Awake () {
        if (instance != null) {
            Debug.Log ("There must only be one MainNetworkManager in the scene!");
            Destroy (this);
        }
        instance = this;
        base.Awake ();
    }
    public void AddPlayerToDictionary (NetworkConnection conn) {
        Debug.Log ($"Conn for: {((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username}: {conn}");
        Players.Add (((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username, conn);
    }
}