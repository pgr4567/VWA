using Mirror;

namespace Minigames {
    public struct CreateMinigameMessage : NetworkMessage {
        public string name;
        public string username;
    }
}