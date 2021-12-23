using Mirror;

namespace Minigames {
    public struct PrepareMinigameMessage : NetworkMessage {
        public string gameID;
        public string[] args;
    }
}