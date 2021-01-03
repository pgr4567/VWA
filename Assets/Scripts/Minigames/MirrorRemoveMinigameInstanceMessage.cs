using Mirror;

namespace Minigames {
    public struct MirrorRemoveMinigameInstanceMessage : NetworkMessage {
        public string name;
        public string gameID;
        public int worldID;
    }
}