using System;
using System.Collections;
using Mirror;
using Networking.RequestMessages;
using UnityEngine;

/*
	Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
	Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

namespace Networking {
    public class MainNetworkAuthenticator : NetworkAuthenticator {
#region Messages
        public struct AuthRequestMessage : NetworkMessage {
            public string username;
            public string password;
        }

        public struct AuthResponseMessage : NetworkMessage {
            public bool authenticated;
            public string sessionToken;
        }
#endregion

#region Server
        /// <summary>
        ///     Called on server from StartServer to initialize the Authenticator
        ///     <para>Server message handlers should be registered in this method.</para>
        /// </summary>
        public override void OnStartServer () {
            // register a handler for the authentication request we expect from client
            NetworkServer.RegisterHandler<AuthRequestMessage> (OnAuthRequestMessage, false);
        }

        /// <summary>
        ///     Called on server from OnServerAuthenticateInternal when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection to client.</param>
        public override void OnServerAuthenticate (NetworkConnection conn) { }

        public void OnAuthRequestMessage (NetworkConnection conn, AuthRequestMessage msg) {
            string loginResponse =
                Helpers.Get ("http://www.vwaspiel.de:3001/login?username=" + msg.username + "&password=" + msg.password);
            if (loginResponse == ServerResponses.Success) {
                string generateSessionTokenResponse = SessionManager.GenerateSessionID (msg.username);
                if (generateSessionTokenResponse == ServerResponses.Success) {
                    string getSessionTokenResponse = SessionManager.GetSessionToken(msg.username);
                    if (getSessionTokenResponse != ServerResponses.UnexpectedError && getSessionTokenResponse != ServerResponses.UsernameNotExist && getSessionTokenResponse != ServerResponses.SessionTimeInvalid) {
                        string getSessionTokenTimeResponse = SessionManager.GetSessionTime(msg.username);
                        if (getSessionTokenTimeResponse == ServerResponses.UnexpectedError ||
                            getSessionTokenTimeResponse == ServerResponses.UsernameNotExist) {
                            return;
                        }
                        AuthResponseMessage authResponseMessage = new AuthResponseMessage { authenticated = true, sessionToken = getSessionTokenResponse };
                        conn.authenticationData = new AuthenticationData
                            { username = msg.username, sessionToken = getSessionTokenResponse, sessionTime = DateTime.Parse (getSessionTokenTimeResponse.Replace ("\"", "")) };
                        conn.isAuthenticated = true;
                        conn.Send (authResponseMessage);

                        // Invoke the event to complete a successful authentication
                        OnServerAuthenticated.Invoke (conn);
                    } else {
                        Disconnect (conn);
                    }
                } else {
                    Disconnect (conn);
                }
            } else {
                Disconnect (conn);
            }
        }

        private IEnumerator Disconnect (NetworkConnection conn) {
            AuthResponseMessage authResponseMessage = new AuthResponseMessage { authenticated = false };
            conn.isAuthenticated = false;
            conn.Send (authResponseMessage);
            
            yield return new WaitForSeconds (1f);
            conn.Disconnect ();
        }
#endregion

#region Client
        /// <summary>
        ///     Called on client from StartClient to initialize the Authenticator
        ///     <para>Client message handlers should be registered in this method.</para>
        /// </summary>
        public override void OnStartClient () {
            // register a handler for the authentication response we expect from server
            NetworkClient.RegisterHandler<AuthResponseMessage> (OnAuthResponseMessage, false);
        }

        /// <summary>
        ///     Called on client from OnClientAuthenticateInternal when a client needs to authenticate
        /// </summary>
        /// <param name="conn">Connection of the client.</param>
        public override void OnClientAuthenticate (NetworkConnection conn) {
            string             username           = PlayerPrefs.GetString ("username");
            string             password           = PlayerPrefs.GetString ("password");
            AuthRequestMessage authRequestMessage = new AuthRequestMessage { username = username, password = password };

            conn.Send (authRequestMessage);
        }

        private void OnAuthResponseMessage (NetworkConnection conn, AuthResponseMessage msg) {
            if (msg.authenticated) // Invoke the event to complete a successful authentication
            {
                PlayerPrefs.SetString ("sessionToken", msg.sessionToken);
                OnClientAuthenticated.Invoke (conn);
            } else {
                conn.Disconnect ();
            }
        }
#endregion
    }
}