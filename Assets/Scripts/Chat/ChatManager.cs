using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Chat;
using Mirror;
using General;
using Networking;
using Networking.RequestMessages;

public class ChatManager : MonoBehaviour {

    private List<Command> commands = new List<Command>() {
        new Command ("addMoney", new string[0], new PermissionTable[] { PermissionTable.CHEAT_MONEY }, (string[] args, string _) => {
            string res =  Helpers.Get("http://vwaspiel.de:3001/addMoney?username=" + args[0] + "&amount=" + args[1]) == ServerResponses.Success ? "Geld wurde erfolgreich hinzugefügt." : "Fehler, Geld konnte nicht hinzugefügt werden!";
            string money = RequestManagerServer.instance.GetMoney(args[0], out ResponseResourceStatus status);
            RequestManagerServer.instance.SendbackResponse(args[0], money, "money", new string[0], status, 0);
            return res;
        }, 2, "/addMoney <username> <amount>"),
        new Command ("addRank", new string[0], new PermissionTable[] { PermissionTable.ADD_RANK }, (string[] args, string _) => {
            return Helpers.Get("http://vwaspiel.de:3001/addRank?name=" + args[0] + "&permissions=" + args[1]) == ServerResponses.Success ? "Rang wurde erfolgreich hinzugefügt." : "Fehler, Rang konnte nicht hinzugefügt werden!";
        }, 2, "/addRank <name> <permissions>"),
        new Command ("removeRank", new string[0], new PermissionTable[] { PermissionTable.REMOVE_RANK }, (string[] args, string _) => {
            return Helpers.Get("http://vwaspiel.de:3001/removeRank?id=" + args[0]) == ServerResponses.Success ? "Rang wurde erfolgreich gelöscht." : "Fehler, Rang konnte nicht gelöscht werden!";
        }, 1, "/removeRank <id>"),
        new Command ("getRankInfo", new string[0], new PermissionTable[] { PermissionTable.GET_RANK_INFO }, (string[] args, string _) => {
            string res = Helpers.Get("http://vwaspiel.de:3001/getRankInfo?id=" + args[0]);
            return res == ServerResponses.UnexpectedError ? "Fehler, Ranginfo konnte nicht erhalten werden!" : res;
        }, 1, "/getRankInfo <id>"),
        new Command ("getRank", new string[0], new PermissionTable[] { PermissionTable.NONE }, (string[] args, string _) => {
            return PermissionManager.GetRankOfUser(args[0]).ToString();
        }, 1, "/getRank <username>"),
        new Command ("setRank", new string[0], new PermissionTable[] { PermissionTable.ASSIGN_RANKS }, (string[] args, string _) => {
            string res = Helpers.Get("http://vwaspiel.de:3001/setRank?username=" + args[0] + "&rank=" + args[1]) == ServerResponses.Success ? "Rang wurde erfolgreich geändert." : "Fehler, Rang konnte nicht geändert werden!";
            PermissionManager.FetchUserRank(args[0]);
            return res;
        }, 2, "/setRank <username> <id>"),
        new Command ("message", new string[] { "msg", "whisper", "w", "tell" }, new PermissionTable[] { PermissionTable.NONE }, (string[] args, string sender) => {
            GameObject player = MainNetworkManager.instance.playerObjs.FirstOrDefault (val => val.Key == args[0]).Value;
            if (player != null) {
                NetworkServer.SendToClientOfPlayer (player.GetComponent<NetworkIdentity> (), new ChatMessage { sender = sender, message = string.Join(" ", args.Skip(1)) });
                return "";
            }
            return "Spieler konnte nicht gefunden werden!";
        }, -1, "/message <username> <msg>")
    };

    private void Start () {
        if (GameManager.instance.isServer) {
            NetworkServer.RegisterHandler<ChatMessage> (msg => {
                if (msg.message.StartsWith("/")) {
                    HandleCommand(msg);
                }
                else {
                    foreach (KeyValuePair<string, GameObject> val in MainNetworkManager.instance.playerObjs.Where (val =>
                        val.Key != msg.sender)) {
                        NetworkServer.SendToClientOfPlayer (val.Value.GetComponent<NetworkIdentity> (), msg);
                    }
                }
            });
        }
    }

    private bool CheckCommand (string cmd, out Command command) {
        command = commands.FirstOrDefault(c => c.name == cmd || c.aliases.Contains(cmd));
        return command != null;
    }

    private void HandleCommand (ChatMessage msg) {
        string[] words = msg.message.Split(' ');
        if (CheckCommand(words[0].Substring(1), out Command command)) {
            if (PermissionManager.CanPlayerDo(msg.sender, command.permissions)) {
                NetworkServer.SendToClientOfPlayer (MainNetworkManager.instance.playerObjs.First(v => v.Key == msg.sender).Value.GetComponent<NetworkIdentity>(), new ChatMessage { sender = "Server", message = command.ExecuteCommand(words.Skip(1).ToArray(), msg.sender) });
            }
            else {
                NetworkServer.SendToClientOfPlayer (MainNetworkManager.instance.playerObjs.First(v => v.Key == msg.sender).Value.GetComponent<NetworkIdentity>(), new ChatMessage { sender = "Server", message = "Du hast nicht genügend Berechtigungen um diesen Befehl auszuführen!" });
            }
        } else {
            NetworkServer.SendToClientOfPlayer (MainNetworkManager.instance.playerObjs.First(v => v.Key == msg.sender).Value.GetComponent<NetworkIdentity>(), new ChatMessage { sender = "Server", message = "Command wurde nicht gefunden!" });
        }
    }
}