using Networking;
using Networking.RequestMessages;
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

        private void Start () { passwordToggle.isOn = PlayerPrefs.GetInt ("passwordToggle") == 1; }

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

            string response = Helpers.Get ("http://www.vwaspiel.de:3000/register?username=" + username.text + "&password=" +
                                           password.text);
            switch (response) {
                case ServerResponses.Success:
                    SaveCredentials ();
                    Login ();
                    break;
                case ServerResponses.UnexpectedError:
                    resultText.text = "Unerwarteter Fehler. Versuche es bitte erneut.";
                    break;
                case ServerResponses.RgUsernameExist:
                    resultText.text = "Dieser Username existiert bereits. Bitte nimm einen anderen.";
                    break;
                case ServerResponses.InvalidUsernamePassword:
                    resultText.text = "Username oder Passwort ist nicht zulässig.";
                    break;
            }
            Debug.Log (response);
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