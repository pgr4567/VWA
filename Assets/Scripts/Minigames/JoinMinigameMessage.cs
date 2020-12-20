using Mirror;

public struct JoinMinigameMessage : NetworkMessage {
    public string username;
    public string gameID;
    public string name;
}