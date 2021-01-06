using System;
using General;
using Mirror;
using UnityEngine;

namespace Networking.RequestMessages {
    public class RequestManagerClient {
        public int maxTries = 5;
        public Action<string> onMoneyRequest;
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
                //TODO: HANDLE OTHER STATI EXCEPT ResponseResourceStatus.SUCCESS
                if (msg.responseStatus == ResponseResourceStatus.SUCCESS) {
                    switch (msg.request) {
                        case "money":
                            onMoneyRequest?.Invoke (msg.response);
                            break;
                    }
                } else if (msg.tryNumber <= maxTries - 1) {
                    NetworkClient.Send (new RequestResourceMessage {
                        username = GameManager.instance.username, sessionToken = GameManager.instance.sessionToken,
                        request  = msg.request, tryNumber                      = msg.tryNumber + 1
                    });
                }
            });
        }

        public void SendRequest (string request) {
            NetworkClient.Send (new RequestResourceMessage { username = GameManager.instance.username, sessionToken = GameManager.instance.sessionToken, request = request, tryNumber = 0 });
        }
    }
}