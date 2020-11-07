using UnityEngine;

public class MouseController : MonoBehaviour {
    public static MouseController instance;
    private void Awake () {
        if (instance != null) {
            Debug.LogWarning ("There must only be one MouseController in the scene.");
            Destroy (this);
        }
        instance = this;
    }
    private void Start () {
        HideCursor ();
    }
    public void ShowCursor () {
        Cursor.lockState = CursorLockMode.None;
    }
    public void HideCursor () {
        Cursor.lockState = CursorLockMode.Locked;
    }
}