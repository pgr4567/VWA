using Mirror;

public class CreateMinigameMessage : MessageBase {
    public string name { get; set; }
    public string username { get; set; }

    public override void Serialize (NetworkWriter writer) {
        writer.WriteString (name);
        writer.WriteString (username);
    }
    public override void Deserialize (NetworkReader reader) {
        name = reader.ReadString ();
        username = reader.ReadString ();
    }
}