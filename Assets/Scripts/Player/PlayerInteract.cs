using Mirror;
using UnityEngine;

//TODO: alles temporär
public class PlayerInteract : MonoBehaviour {
    private void Update () {
        //TODO: neues Input System
        if (Input.GetMouseButtonDown (0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            if (Physics.Raycast (ray, out hit)) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("TempStartGame")) {
                    NetworkClient.Send (new CreateMinigameMessage () { name = "2v2 Soccer", username = PlayerPrefs.GetString ("username") });
                }
            }
        }
    }
}