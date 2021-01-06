using System;
using Mirror;

namespace Networking.RequestMessages {
    public class RequestManagerServer {
        public static RequestManagerServer instance;

        public RequestManagerServer () {
            if (instance != null) {
                throw new InvalidOperationException ("There must only ever be one RequestManager!");
            }

            instance = this;
            SetupHandlers ();
        }

        private void SetupHandlers () {
            NetworkServer.RegisterHandler<RequestResourceMessage> (msg => {
                if (msg.username == null || msg.sessionToken == null) {
                    return;
                }

                if (!SessionManager.CheckValidateSession (msg.username, msg.sessionToken)) {
                    return;
                }

                string                 response;
                ResponseResourceStatus status;
                switch (msg.request) {
                    case "money":
                        response = GetMoney (msg.username, out status);
                        break;
                    default:
                        response = "";
                        status   = ResponseResourceStatus.NOT_FOUND;
                        break;
                }

                SendbackResponse (msg.username, response, msg.request, status, msg.tryNumber);
            });
        }

        private string GetMoney (string username, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/getMoney?username=" + username);
            status = response != ServerResponses.UnexpectedError && response != ServerResponses.UsernameNotExist &&
                     response.StartsWith ("$")
                ? ResponseResourceStatus.SUCCESS
                : ResponseResourceStatus.ERROR;
            return response;
        }

        private void SendbackResponse (string username, string response, string request, ResponseResourceStatus status,
            int tryNumber) {
            MainNetworkManager.instance.players[username].Send (new RequestResourceMessage
                { responseStatus = status, response = response, request = request, tryNumber = tryNumber });
        }
    }
}