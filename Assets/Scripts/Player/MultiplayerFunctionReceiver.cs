using General;
using Minigames;
using Mirror;

namespace Player {
    public class MultiplayerFunctionReceiver : NetworkBehaviour {
        private void Start () {
            if (isLocalPlayer) {
                RegisterEventHandlers ();
            }
        }

        private void RegisterEventHandlers () {
            NetworkClient.RegisterHandler<MirrorMinigameMessage> (CreateMinigame);
            NetworkClient.RegisterHandler<FreezeMovementMessage> (msg => {
                GameManager.instance.isInLobby = msg.freeze;
            });
            NetworkClient.RegisterHandler<MirrorRemoveMinigameInstanceMessage> (msg => {
                MinigameDispatcher.instance.RemoveMinigameInstanceClient (msg.name, msg.worldID, msg.gameID);
            });
            NetworkClient.RegisterHandler<StartMinigameMessage> (msg => {
                MinigameDispatcher.instance.GetMinigameManagerForGame (msg.gameID)?.StartGameClient ();
            });
            NetworkClient.RegisterHandler<MinigameListUpdateMessage> (msg => {
                GameManager.instance.UpdateMinigameList (msg.gameIDs);
            });
        }

        private void CreateMinigame (MirrorMinigameMessage msg) {
            string name   = msg.name;
            int    number = msg.number;

            MinigameManager manager = MinigameDispatcher.instance.CreateMinigameInstanceClient (name, number);
            if (manager == null) // World was not created.
            { }
        }
    }
}