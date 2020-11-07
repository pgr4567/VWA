using System.Collections;
using Mirror;
using UnityEngine;

/*
	Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
	Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class MainNetworkAuthenticator : NetworkAuthenticator {
    #region Messages

    public class AuthRequestMessage : MessageBase {
        public string username { get; set; }
        public string password { get; set; }

        public override void Serialize (NetworkWriter writer) {
            writer.WriteString (username);
            writer.WriteString (password);
        }
        public override void Deserialize (NetworkReader reader) {
            username = reader.ReadString ();
            password = reader.ReadString ();
        }
    }

    public class AuthResponseMessage : MessageBase {
        public bool authenticated { get; set; }

        public override void Serialize (NetworkWriter writer) {
            writer.WriteBoolean (authenticated);
        }
        public override void Deserialize (NetworkReader reader) {
            authenticated = reader.ReadBoolean ();
        }
    }

    #endregion

    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer () {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage> (OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate (NetworkConnection conn) { }

    public void OnAuthRequestMessage (NetworkConnection conn, AuthRequestMessage msg) {
        string response = Helpers.Get ("http://vwaspiel.de:3000/login?username=" + msg.username + "&password=" + msg.password);
        if (response == "SUCCESS") {
            AuthResponseMessage authResponseMessage = new AuthResponseMessage () { authenticated = true };

            conn.authenticationData = msg;
            conn.isAuthenticated = true;
            conn.Send (authResponseMessage);

            // Invoke the event to complete a successful authentication
            OnServerAuthenticated.Invoke (conn);
        } else {
            AuthResponseMessage authResponseMessage = new AuthResponseMessage () { authenticated = false };

            conn.authenticationData = msg;
            conn.isAuthenticated = false;
            conn.Send (authResponseMessage);

            Disconnect (conn);
        }
    }
    private IEnumerator Disconnect (NetworkConnection conn) {
        yield return new WaitForSeconds (1f);
        conn.Disconnect ();
    }

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient () {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage> (OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection of the client.</param>
    public override void OnClientAuthenticate (NetworkConnection conn) {
        string username = PlayerPrefs.GetString("username");
        string password = PlayerPrefs.GetString("password");
        AuthRequestMessage authRequestMessage = new AuthRequestMessage () { username = username, password = password };

        conn.Send (authRequestMessage);
    }

    public void OnAuthResponseMessage (NetworkConnection conn, AuthResponseMessage msg) {
        if (msg.authenticated == true) {
            // Invoke the event to complete a successful authentication
            OnClientAuthenticated.Invoke (conn);
        } else {
            conn.Disconnect ();
        }
    }

    #endregion
}