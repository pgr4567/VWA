﻿using Mirror;

namespace Networking.RequestMessages {
    public struct RequestResourceMessage : NetworkMessage {
        public string username;
        public string sessionToken;
        public string request;
        public ResponseResourceStatus responseStatus;
        public string response;
        public int tryNumber;
    }
}