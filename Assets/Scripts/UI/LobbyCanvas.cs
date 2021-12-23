using System.Collections.Generic;
using General;
using Minigames;
using Mirror;
using Player;
using TMPro;
using UnityEngine;

namespace UI {
    public class LobbyCanvas : MonoBehaviour {
        [SerializeField] private TMP_Text lobbyHeader;
        [SerializeField] private TMP_Text gameDescription;
        [SerializeField] private TMP_Text gameStats;

        [SerializeField] private GameObject teamPlayerPrefab;

        //TODO: Add support for more teams:
        [SerializeField] private Transform team1;
        [SerializeField] private Transform team2;
        [SerializeField] private GameObject ownerControls;
        private string _gameID = string.Empty;

        public void ShowCanvas (string header, string desc, string stats, List<string> team1, List<string> team2,
            bool isOwner, string gameID) {
            lobbyHeader.text     = header;
            gameDescription.text = desc;
            gameStats.text       = stats;
            _gameID              = gameID;

            foreach (string s in team1) {
                AddPlayer (1, s);
            }

            foreach (string s in team2) {
                AddPlayer (2, s);
            }

            ownerControls.SetActive (isOwner);
        }

        public void UpdateIsOwner (bool isOwner) { ownerControls.SetActive (isOwner); }

        public void UpdateTeams (List<string> team1, List<string> team2) {
            RemoveAllPlayers ();
            foreach (string s in team1) {
                AddPlayer (1, s);
            }

            foreach (string s in team2) {
                AddPlayer (2, s);
            }
        }

        public void AddPlayer (int team, string player) {
            GameObject go = Instantiate (teamPlayerPrefab, team == 1 ? team1 : team2);
            go.GetComponent<TMP_Text> ().text = player;
        }

        public void RemoveAllPlayers () {
            foreach (Transform c in team1) {
                Destroy (c.gameObject);
            }

            foreach (Transform c in team2) {
                Destroy (c.gameObject);
            }
        }

        public void StartGame () {
            NetworkClient.Send (new StartMinigameMessage { gameID = _gameID });
            MouseController.instance.HideCursor ();
            gameObject.SetActive (false);
        }

        public void LeaveLobby () {
            MouseController.instance.HideCursor ();
            GameManager.instance.LeaveMinigame();
            gameObject.SetActive (false);
        }
    }
}