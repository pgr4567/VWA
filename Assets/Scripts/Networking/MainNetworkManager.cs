using System.Collections.Generic;
using System.Threading.Tasks;
using Minigames;
using Mirror;
using UnityEngine;

namespace Networking {
    public class MainNetworkManager : NetworkManager {
        public static MainNetworkManager instance;
        public GameObject localPlayer;
        public GameObject remotePlayer;
        public Dictionary<string, GameObject> playerObjs = new Dictionary<string, GameObject> ();
        public Dictionary<string, NetworkConnection> players = new Dictionary<string, NetworkConnection> ();

        public override void Awake () {
            if (instance != null) {
                Debug.Log ("There must only be one MainNetworkManager in the scene!");
                Destroy (this);
            }

            instance = this;
            base.Awake ();
        }

        private void AddPlayerToDictionary (NetworkConnection conn) {
            string username = ((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username;
            Debug.Log ($"Trying to connect: {username}");
            if (players.ContainsKey (username)) {
                conn.Disconnect ();
                Debug.LogWarning ("Somebody who is already logged in, tried to log in.");
                return;
            }

            Debug.Log ($"Conn for: {username}: {conn}");
            players.Add (username, conn);

            CreatePlayerObject (conn, username);
        }

        public void RemovePlayerFromDictionary (NetworkConnection conn) {
            string username = ((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username;
            players.Remove (username);
            playerObjs.Remove (username);
        }

        public override void OnServerConnect (NetworkConnection conn) {
            AddPlayerToDictionary (conn);

            SendPlayerMinigameList (conn);
        }

        public override void OnServerDisconnect (NetworkConnection conn) {
            RemovePlayerFromDictionary (conn);

            base.OnServerDisconnect (conn);
        }

        public override void OnClientDisconnect (NetworkConnection conn) {
            //TODO: TEMPORARY
            NetworkClient.Send (new LeaveMinigameMessage
                { username = PlayerPrefs.GetString ("username"), gameID = "(27.0, 0.0, 7.0)Labyrinth" });

            base.OnClientDisconnect (conn);
        }

        private async void SendPlayerMinigameList (NetworkConnection conn) {
            await Task.Delay (500);
            string username = ((MainNetworkAuthenticator.AuthRequestMessage)conn.authenticationData).username;
            MinigameDispatcher.instance.SendMinigameListUpdateToClient (playerObjs[username]
                .GetComponent<NetworkIdentity> ());
        }

        private void CreatePlayerObject (NetworkConnection conn, string username) {
            Debug.Log ($"GO for: {username}: {conn}");
            GameObject go = Instantiate (localPlayer, transform.position, Quaternion.identity);
            playerObjs.Add (username, go);

            NetworkServer.AddPlayerForConnection (conn, go);
        }
    }
}