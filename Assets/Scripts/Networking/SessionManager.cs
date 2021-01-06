using System;
using Networking.RequestMessages;
using UnityEngine;

namespace Networking {
    public static class SessionManager {
        private static readonly int maxSessionTime;
        static SessionManager () {
            maxSessionTime = int.Parse (Helpers.Get ("http://vwaspiel.de:3001/maxSessionTime").Substring (1));
        }
        
        public static string GetSessionToken (string username) {
            return Helpers.Get ("http://vwaspiel.de:3001/getSessionToken?username=" + username);
        }
        
        public static string GetSessionTime (string username) {
            return Helpers.Get ("http://vwaspiel.de:3001/getSessionTokenTime?username=" + username);
        }

        public static string GenerateSessionID (string username) {
            return Helpers.Get ("http://vwaspiel.de:3001/generateSessionToken?username=" + username);
        }

        public static bool CheckValidateSession (string username, string sessionToken) {
            AuthenticationData authData = (AuthenticationData)MainNetworkManager.instance.players[username].authenticationData;
            if (authData.sessionToken == sessionToken &&
                authData.sessionTime > DateTime.Now.AddHours (-maxSessionTime)) {
                return true;
            }
            string result = GetSessionToken (username);
            if (result != ServerResponses.UnexpectedError && result != ServerResponses.UsernameNotExist &&
                result != ServerResponses.SessionTimeInvalid && result == sessionToken) {
                return true;
            }
            
            MainNetworkManager.instance.DisconnectClient (username);
            return false;
        }
    }
}