using Mirror;

namespace Minigames {
    public struct ReadyUpMessage : NetworkMessage {
        public string username;
        public string gameID;
    }
}