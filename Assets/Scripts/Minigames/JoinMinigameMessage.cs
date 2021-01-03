using Mirror;

namespace Minigames {
    public struct JoinMinigameMessage : NetworkMessage {
        public string username;
        public string gameID;
    }
}