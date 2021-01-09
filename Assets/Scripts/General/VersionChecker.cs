using System.Threading;
using Networking;
using TMPro;
using UnityEngine;

namespace General {
    public class VersionChecker : MonoBehaviour {
        public static VersionChecker instance;
        [SerializeField] private TextAsset versionFile;
        [SerializeField] private GameObject[] canvases;
        [SerializeField] private GameObject invalidVersionCanvas;
        [SerializeField] private TMP_Text yourVersionText;
        [SerializeField] private TMP_Text serverVersionText;
        private string _localVersion;
        private Thread _checkVersionThread;
        public bool hasCheckedVersion;
        private void Awake () {
            if (instance != null) {
                Debug.LogWarning("There must only ever be one VersionChecker.");
                Destroy (gameObject);
            }

            _localVersion       = versionFile.text;
            instance            = this;
            _checkVersionThread = new Thread (CheckVersion);
            _checkVersionThread.Start ();
        }

        public void CheckVersion () {
            string version = Helpers.Get ("http://vwaspiel.de:3000/version").Replace("\n", "");
            if (_localVersion != version) {
                foreach (GameObject go in canvases) {
                    go.SetActive(false);
                }

                yourVersionText.text   = "Deine Version ist: " + versionFile.text;
                serverVersionText.text = "Die Version des Servers ist: " + version;
                invalidVersionCanvas.SetActive(true);
            } else {
                hasCheckedVersion = true;
            }
            _checkVersionThread.Abort();
        }

        public void QuitGame () {
            Application.Quit();
        }
    }
}