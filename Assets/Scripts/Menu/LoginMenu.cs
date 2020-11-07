using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour {
    [SerializeField] private MainNetworkManager networkManager;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private Toggle passwordToggle;
    [SerializeField] private GameObject registerCanvas;

    private void OnEnable () {
        LoadCredentials ();
    }

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
        username.text = PlayerPrefs.GetString ("username");
        password.text = PlayerPrefs.GetString ("password");
        passwordToggle.isOn = PlayerPrefs.GetInt ("passwordToggle") == 1 ? true : false;
    }

    public void StartHost () {
        SaveCredentials ();
        networkManager.StartHost ();
    }
    public void StartClient () {
        SaveCredentials ();
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