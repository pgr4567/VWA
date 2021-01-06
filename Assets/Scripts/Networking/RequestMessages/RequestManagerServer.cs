using System;
using System.Linq;
using Mirror;
using Shops;

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
                    case "getInventory":
                        response = GetInventory (msg.username, out status);
                        break;
                    case "trybuy":
                        response = TryBuy (msg.username, msg.args, out status);
                        break;
                    default:
                        response = "";
                        status   = ResponseResourceStatus.NOT_FOUND;
                        break;
                }

                SendbackResponse (msg.username, response, msg.request, msg.args, status, msg.tryNumber);
            });
        }

        private string GetMoney (string username, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/getMoney?username=" + username);
            switch (response) {
                case ServerResponses.UnexpectedError:
                    status = ResponseResourceStatus.ERROR;
                    break;
                case ServerResponses.UsernameNotExist:
                    status = ResponseResourceStatus.FORBIDDEN;
                    break;
                default:
                    status = ResponseResourceStatus.SUCCESS;
                    break;
            }

            return response;
        }

        private string GetInventory (string username, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/getInventory?username=" + username);
            switch (response) {
                case ServerResponses.UnexpectedError:
                    status = ResponseResourceStatus.ERROR;
                    break;
                case ServerResponses.UsernameNotExist:
                    status = ResponseResourceStatus.FORBIDDEN;
                    break;
                default:
                    status = ResponseResourceStatus.SUCCESS;
                    break;
            }

            return response;
        }

        private string TryBuy (string username, string[] args, out ResponseResourceStatus status) {
            string item = args[0];
            if (ShopManager.items.Count (i => i.name == item) == 0) {
                status = ResponseResourceStatus.FORBIDDEN;
                return "";
            }

            if (!ShopManager.items.First (i => i.name == item).sellable) {
                status = ResponseResourceStatus.FORBIDDEN;
                return "";
            }

            int price = ShopManager.items.First (i => i.name == item).price;
            string response = Helpers.Get ("http://vwaspiel.de:3001/tryBuy?username=" + username + "&item=" +
                                           item + "&price=" + price);
            status = GetStatusSimple (response);
            return response;
        }

        private ResponseResourceStatus GetStatusSimple (string response) {
            return response == ServerResponses.Success
                ? ResponseResourceStatus.SUCCESS
                : response == ServerResponses.UnexpectedError
                    ? ResponseResourceStatus.ERROR
                    : ResponseResourceStatus.FORBIDDEN;
        }

        private void SendbackResponse (string username, string response, string request, string[] args,
            ResponseResourceStatus status,
            int tryNumber) {
            MainNetworkManager.instance.players[username].Send (new RequestResourceMessage {
                responseStatus = status, response = response, request = request, args = args, tryNumber = tryNumber
            });
        }
    }
}