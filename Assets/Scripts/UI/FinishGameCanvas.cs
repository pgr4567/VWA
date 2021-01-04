using System.Linq;
using General;
using Minigames;
using Player;
using TMPro;
using UnityEngine;

namespace UI {
    public class FinishGameCanvas : MonoBehaviour {
        [SerializeField] private TMP_Text minigameName;
        [SerializeField] private TMP_Text teamName;

        public void ShowCanvas (Minigame minigame, MinigameTeam team) {
            minigameName.text = minigame.name;
            teamName.text     = team.players.First ();
        }

        public void HideCanvas () {
            GameManager.instance.isInGUI = false;
            MouseController.instance.HideCursor ();
            gameObject.SetActive (false);
        }
    }
}