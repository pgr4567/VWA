using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu {
    public class LoginMenu : MonoBehaviour {
        [SerializeField] private MainNetworkManager networkManager;
        [SerializeField] private TMP_InputField username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private Toggle passwordToggle;
        [SerializeField] private GameObject registerCanvas;
        private bool _starting;

        private void OnEnable () { LoadCredentials (); }

        private void SaveCredentials () {
            PlayerPrefs.SetString ("username", username.text);
            if (passwordToggle.isOn) {
                PlayerPrefs.SetString ("password", password.text);
                PlayerPrefs.SetInt ("passwordToggle", 1);
            } else {
                PlayerPrefs.DeleteKey ("password");
                PlayerPrefs.SetInt ("passwordToggle", 0);
            }
        }

        private void LoadCredentials () {
            username.text       = PlayerPrefs.GetString ("username");
            password.text       = PlayerPrefs.GetString ("password");
            passwordToggle.isOn = PlayerPrefs.GetInt ("passwordToggle") == 1 ? true : false;
        }

        public void StartServer () { networkManager.StartServer (); }

        public void StartClient () {
            if (_starting) {
                return;
            }

            _starting = true;
            SaveCredentials ();
            networkManager.networkAddress = "217.160.213.47";
            networkManager.StartClient ();
        }

        public void StartLocalClient () {
            if (_starting) {
                return;
            }

            _starting = true;
            SaveCredentials ();
            networkManager.networkAddress = "localhost";
            networkManager.StartClient ();
        }

        public void PasswordToggle (bool isOn) {
            if (!isOn) {
                PlayerPrefs.DeleteKey ("password");
                PlayerPrefs.SetInt ("passwordToggle", 0);
            } else {
                PlayerPrefs.SetInt ("passwordToggle", 1);
            }
        }

        public void Register () {
            registerCanvas.SetActive (true);
            gameObject.SetActive (false);
        }
    }
}