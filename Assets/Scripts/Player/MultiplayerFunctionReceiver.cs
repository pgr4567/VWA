using Mirror;

public class MultiplayerFunctionReceiver : NetworkBehaviour {
    private void Start () {
        if (isLocalPlayer && !isServer) {
            RegisterEventHandlers ();
        }
    }
    private void RegisterEventHandlers () {
        NetworkClient.RegisterHandler<MirrorMinigameMessage> (CreateMinigame);
    }
    private void CreateMinigame (MirrorMinigameMessage msg) {
        UnityEngine.Debug.Log("Received MirrorMinigameMessage!");
        string name = msg.name;
        int number = msg.number;

        MinigameManager manager = MinigameDispatcher.instance.CreateMinigameInstanceClient (name, number);
        if (manager == null) {
            // World was not created.
            return;
        }
    }

}