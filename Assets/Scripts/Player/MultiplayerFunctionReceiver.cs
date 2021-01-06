using Chat;
using General;
using Minigames;
using Mirror;
using Networking;
using UI;
using UnityEngine;

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
                GameManager.instance.isInGUI = msg.freeze;
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
            NetworkClient.RegisterHandler<ChatMessage> (msg => { ChatCanvas.instance.AppendMessage (msg); });
            NetworkClient.RegisterHandler<SessionInvalidatedMessage>(msg => { Debug.LogWarning ("Should display something here."); });
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