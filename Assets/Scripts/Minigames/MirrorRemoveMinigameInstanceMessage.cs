using Mirror;

public struct MirrorRemoveMinigameInstanceMessage : NetworkMessage {
    public string name;
    public string gameID;
    public int worldID;
}