using Mirror;

public class MirrorMinigameMessage : MessageBase {
    public string name { get; set; }
    public int number { get; set; }

    public override void Serialize (NetworkWriter writer) {
        writer.WriteString (name);
        writer.WriteInt32 (number);
    }
    public override void Deserialize (NetworkReader reader) {
        name = reader.ReadString ();
        number = reader.ReadInt32 ();
    }
}