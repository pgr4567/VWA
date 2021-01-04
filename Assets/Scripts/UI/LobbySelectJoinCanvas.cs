using System.Threading.Tasks;
using General;
using Minigames;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class LobbySelectJoinCanvas : MonoBehaviour {
        [SerializeField] private GameObject minigamePrefab;
        [SerializeField] private Transform minigameParent;

        private async void Start () {
            //TODO: WHYYYYYYY
            await Task.Delay (200);
            if (GameManager.instance.isServer) {
                return;
            }

            GameManager.instance.onMinigameListUpdate += UpdateMinigameList;
        }

        public void JoinGame (string gameID) {
            if (GameManager.instance.isServer) {
                return;
            }

            NetworkClient.Send (
                new JoinMinigameMessage { username = GameManager.instance.username, gameID = gameID });
        }

        public void UpdateMinigameList () {
            //TODO: TEMPORARY
            foreach (Transform c in minigameParent) {
                Destroy (c.gameObject);
            }

            foreach (string gameID in GameManager.instance.minigameIDs) {
                GameObject go = Instantiate (minigamePrefab, minigameParent);
                go.GetComponent<Button> ().onClick.AddListener (() => { JoinGame (gameID); });
                go.GetComponentInChildren<TMP_Text> ().text = gameID.Split (')')[1];
            }
        }
    }
}