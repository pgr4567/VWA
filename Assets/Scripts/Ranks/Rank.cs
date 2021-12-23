using System.Collections.Generic;

public class Rank {
    public int id;
    public string name;
    public List<int> permissions;

    public Rank(int id, string name, List<int> permissions) {
        this.id = id;
        this.name = name;
        this.permissions = permissions;
    }

    public override string ToString() {
        return id + "|" + name;
    }
}