using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu {
    public class RegisterMenu : MonoBehaviour {
        [SerializeField] private GameObject loginCanvas;
        [SerializeField] private TMP_InputField username;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private TMP_InputField passwordRepeat;
        [SerializeField] private Toggle passwordToggle;
        [SerializeField] private TMP_Text resultText;

        private void Start () { passwordToggle.isOn = PlayerPrefs.GetInt ("passwordToggle") == 1 ? true : false; }

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

        public void Register () {
            resultText.text = string.Empty;
            if (password.text != passwordRepeat.text) {
                resultText.text = "Passwörter stimmen nicht überein!";
                return;
            }

            string response = Helpers.Get ("http://vwaspiel.de:3000/register?username=" + username.text + "&password=" +
                                           password.text);
            if (response == "SUCCESS") {
                SaveCredentials ();
                Login ();
            } else if (response == "UNEXPECTED ERROR") {
                resultText.text = "Unerwarteter Fehler. Versuche es bitte erneut.";
            } else if (response == "ERROR: USERNAME EXISTS") {
                resultText.text = "Dieser Username existiert bereits. Bitte nimm einen anderen.";
            }
        }

        public void PasswordToggle (bool isOn) {
            if (!isOn) {
                PlayerPrefs.DeleteKey ("password");
                PlayerPrefs.SetInt ("passwordToggle", 0);
            } else {
                PlayerPrefs.SetInt ("passwordToggle", 1);
            }
        }

        public void Login () {
            resultText.text = string.Empty;
            loginCanvas.SetActive (true);
            gameObject.SetActive (false);
        }
    }
}