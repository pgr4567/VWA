using Mirror;

namespace Minigames {
    public struct StartMinigameMessage : NetworkMessage {
        public string gameID;
    }
}