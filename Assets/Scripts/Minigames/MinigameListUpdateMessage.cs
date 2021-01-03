using Mirror;

namespace Minigames {
    public struct MinigameListUpdateMessage : NetworkMessage {
        public string[] gameIDs;
    }
}