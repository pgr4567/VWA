using Mirror;

namespace Chat {
    public struct ChatMessage : NetworkMessage {
        public string sender;
        public string message;
    }
}