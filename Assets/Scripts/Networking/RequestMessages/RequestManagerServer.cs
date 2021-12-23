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
                    case "removeFriend":
                        response = RemoveFriend (msg.username, msg.args, out status);
                        break;
                    case "inviteFriend":
                        response = InviteFriend (msg.username, msg.args, out status);
                        break;
                    case "acceptRequest":
                        response = AcceptFriendRequest (msg.username, msg.args, out status);
                        break;
                    case "declineRequest":
                        response = DeclineFriendRequest (msg.username, msg.args, out status);
                        break;
                    case "removeRequest":
                        response = RemoveRequest (msg.username, msg.args, out status);
                        break;
                    case "friends":
                        response = GetFriends (msg.username, msg.args, out status);
                        break;
                    case "requests":
                        response = GetRequests (msg.username, msg.args, out status);
                        break;
                    case "sentRequests":
                        response = GetSentRequests (msg.username, msg.args, out status);
                        break;
                    case "addRequest":
                        response = AddRequest(msg.username, msg.args, out status);
                        break;
                    default:
                        response = "";
                        status   = ResponseResourceStatus.NOT_FOUND;
                        break;
                }

                SendbackResponse (msg.username, response, msg.request, msg.args, status, msg.tryNumber);
            });
        }

        private string RemoveFriend (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/removeFriend?username=" + username + "&friend=" + args[0]);
            
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
            
            SendbackResponse(args[0], ServerResponses.Success, "notifyRemove", new [] { username }, ResponseResourceStatus.SUCCESS, 0);
            return response;
        }
        
        private string InviteFriend (string username, string[] args, out ResponseResourceStatus status) {
            SendbackResponse(args[0], ServerResponses.Success, "notifyInvite", new [] {username, args[1]}, ResponseResourceStatus.SUCCESS, 0);
            status = ResponseResourceStatus.SUCCESS;
            return ServerResponses.Success;
        }

        private string GetFriends (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/getFriends?username=" + username);

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
        private string GetRequests (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/getFriendRequests?username=" + username);

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
        private string GetSentRequests (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/getSentFriendRequests?username=" + username);

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
        private string AddRequest (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/addFriendRequest?username=" + username + "&friend=" + args[0]);
            
            status = GetStatusSimple(response);
            SendbackResponse(args[0], ServerResponses.Success, "notifyRequest", new [] {username}, ResponseResourceStatus.SUCCESS, 0);
            return response;
        }
        
        private string AcceptFriendRequest (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/acceptFriendRequest?username=" + username + "&friend=" + args[0]);
            
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
            SendbackResponse(args[0], ServerResponses.Success, "notifyAccept", new [] {username}, ResponseResourceStatus.SUCCESS, 0);
            return response;
        }
        
        private string DeclineFriendRequest (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/declineFriendRequest?username=" + username + "&friend=" + args[0]);
            
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
            SendbackResponse(args[0], ServerResponses.Success, "notifyDecline", new [] {username}, ResponseResourceStatus.SUCCESS, 0);
            return response;
        }
        
        private string RemoveRequest (string username, string[] args, out ResponseResourceStatus status) {
            string response = Helpers.Get ("http://vwaspiel.de:3001/removeFriendRequest?username=" + username + "&friend=" + args[0]);
            
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
            SendbackResponse(args[0], ServerResponses.Success, "notifyRequestRemove", new [] {username}, ResponseResourceStatus.SUCCESS, 0);
            return response;
        }

        public string GetMoney (string username, out ResponseResourceStatus status) {
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

        public void SendbackResponse (string username, string response, string request, string[] args,
            ResponseResourceStatus status,
            int tryNumber) {
            if (!MainNetworkManager.instance.players.ContainsKey(username)) {
                return;
            }
            MainNetworkManager.instance.players[username].Send (new RequestResourceMessage {
                responseStatus = status, response = response, request = request, args = args, tryNumber = tryNumber
            });
        }
    }
}