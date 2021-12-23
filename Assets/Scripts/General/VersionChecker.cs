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
        [SerializeField] private GameObject notYetRunCanvas;
        private string _localVersion;
        private Thread _checkVersionThread;
        public bool hasCheckedVersion { get; private set; } = false;
        public bool isCorrectVersion { get; private set; } = false;
        private string serverVersion = "";
        private void Awake () {
            if (instance != null) {
                Debug.LogWarning ("There must only ever be one VersionChecker.");
                Destroy (gameObject);
            }

            _localVersion = versionFile.text;
            instance = this;
            _checkVersionThread = new Thread (CheckVersion);
            _checkVersionThread.Start ();
        }

        public void ShowInvalidVersionScreen () {
            foreach (GameObject go in canvases) {
                go.SetActive (false);
            }

            yourVersionText.text = "Deine Version ist: " + _localVersion;
            serverVersionText.text = "Die Version des Servers ist: " + serverVersion;
            invalidVersionCanvas.SetActive (true);
        }
        public void ShowVersionCheckerNotYetRunScreen () {
            foreach (GameObject go in canvases) {
                go.SetActive (false);
            }
            notYetRunCanvas.SetActive (true);
        }

        public void CloseVersionCheckerNotYetRunScreen () {
            notYetRunCanvas.SetActive (false);
            foreach (GameObject go in canvases) {
                if (go.GetComponent<Menu.LoginMenu> () != null) {
                    go.SetActive (true);
                }
            }
        }

        public void CheckVersion () {
            string version = Helpers.Get ("http://www.vwaspiel.de:3000/version").Replace ("\n", "");
            isCorrectVersion = _localVersion == version;
            serverVersion = version;
            hasCheckedVersion = true;
            _checkVersionThread.Abort ();
        }

        public void QuitGame () {
            Application.Quit ();
        }
    }
}