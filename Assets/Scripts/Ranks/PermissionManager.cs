using System.Collections.Generic;
using Networking;
using System.Linq;

public static class PermissionManager {
    private static Dictionary<int, Rank> ranks = new Dictionary<int, Rank>();
    private static Dictionary<string, int> userRanks = new Dictionary<string, int>();
    public static void FetchRank (int id) {
        string[] data = Helpers.Get ("http://vwaspiel.de:3001/getRankInfo?id=" + id).Split('|');
        if (ranks.ContainsKey(id)) {
            ranks[id].permissions.Clear();
            ranks[id].permissions.AddRange(data[2].Split(';').Where(i => i != null && i.Trim() != "").Select(p => int.Parse(p)));
        }
        else {
            ranks.Add(id, new Rank(id, data[1], new List<int>(data[2].Split(';').Where(i => i != null && i.Trim() != "").Select(p => int.Parse(p)))));
        }
    }
    public static void FetchUserRank (string username) {
        string data = Helpers.Get ("http://vwaspiel.de:3001/getRank?username=" + username).Substring(1);
        if (!userRanks.ContainsKey(username)) {
            userRanks.Add(username, int.Parse(data));
        } else {
            userRanks[username] = int.Parse(data);
        }
    }
    public static bool CanPlayerDo(string username, PermissionTable[] permissions) {
        if (!userRanks.ContainsKey(username)) {
            FetchUserRank (username);
        }
        if (!ranks.ContainsKey(userRanks[username])) {
            FetchRank (userRanks[username]);
        }
        if (ranks[userRanks[username]].permissions.Contains(-1)) {
            return true;
        }
        foreach (PermissionTable p in permissions) {
            if (!ranks[userRanks[username]].permissions.Contains((int)p)) {
                FetchRank (userRanks[username]);
                return false;
            }
        }
        return true;
    }
    public static Rank GetRankOfUser (string username) {
        if (!userRanks.ContainsKey(username)) {
            FetchUserRank (username);
        }
        if (!ranks.ContainsKey(userRanks[username])) {
            FetchRank (userRanks[username]);
        }
        return ranks[userRanks[username]];
    }
    public static Rank GetRankOfId (int id) {
        if (!ranks.ContainsKey(id)) {
            FetchRank (id);
            return null;
        }
        return ranks[id];
    }
}