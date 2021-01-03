using Mirror;

namespace Minigames {
    public struct LeaveMinigameMessage : NetworkMessage {
        public string username;
        public string gameID;
    }
}