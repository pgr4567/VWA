using System;
using General;
using Mirror;
using UnityEngine;

namespace Networking.RequestMessages {
    public class RequestManagerClient {
        public int maxTries = 5;
        public Action<string, string, string[]> onResponse;
        public static RequestManagerClient instance;

        public RequestManagerClient () {
            if (instance != null) {
                throw new InvalidOperationException ("There must only ever be one RequestManager!");
            }

            instance = this;
            SetupHandlers ();
        }

        private void SetupHandlers () {
            NetworkClient.RegisterHandler<RequestResourceMessage> (msg => {
                Debug.Log ("Receiving response: " + msg.response + ", for request: " + msg.request + ", status is: " + msg.responseStatus);
                if (msg.responseStatus == ResponseResourceStatus.SUCCESS) {            
                    onResponse?.Invoke (msg.request, msg.response, msg.args);
                }
                else if (msg.responseStatus == ResponseResourceStatus.FORBIDDEN) {
                    //TODO: MAYBE onForbidden?
                }
                else if (msg.responseStatus == ResponseResourceStatus.ERROR && msg.tryNumber <= maxTries - 1) {
                    NetworkClient.Send (new RequestResourceMessage {
                        username = GameManager.instance.username, sessionToken = GameManager.instance.sessionToken,
                        request  = msg.request, args                           = msg.args, tryNumber = msg.tryNumber + 1
                    });
                }
                else if (msg.responseStatus == ResponseResourceStatus.NOT_FOUND) {
                    Debug.LogWarning("Issued request: " + msg.request + ", but was not found on server.");
                }
            });
        }

        public void SendRequest (string request, params string[] args) {
            Debug.Log ("Sending request: " + request);
            NetworkClient.Send (new RequestResourceMessage {
                username = GameManager.instance.username, sessionToken = GameManager.instance.sessionToken,
                request  = request, args                               = args, tryNumber = 0
            });
        }
    }
}