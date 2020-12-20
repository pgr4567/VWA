using Mirror;

public struct LeaveMinigameMessage : NetworkMessage {
    public string username;
    public string gameID;
}