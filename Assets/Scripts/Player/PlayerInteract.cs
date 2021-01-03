using General;
using Minigames;
using Mirror;
using UnityEngine;

//TODO: alles temporär
namespace Player {
    public class PlayerInteract : NetworkBehaviour {
        private void Update () {
            if (!isLocalPlayer || GameManager.instance.isInLobby) {
                return;
            }

            //TODO: neues Input System
            if (Input.GetMouseButtonDown (0)) {
                Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                if (!Physics.Raycast (ray, out RaycastHit hit)) {
                    return;
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("TempStartGame")) {
                    NetworkClient.Send (new CreateMinigameMessage
                        { name = "Labyrinth", username = PlayerPrefs.GetString ("username") });
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("TempLeaveGame")) {
                    NetworkClient.Send (new LeaveMinigameMessage
                        { username = PlayerPrefs.GetString ("username"), gameID = "(27.0, 0.0, 7.0)Labyrinth" });
                }
            }
        }
    }
}