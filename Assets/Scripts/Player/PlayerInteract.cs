using General;
using Minigames;
using Mirror;
using UnityEngine;
using Minigames.Labyrinth.Maze;

//TODO: alles temporär
namespace Player {
    public class PlayerInteract : NetworkBehaviour {
        private void Update () {
            if (!isLocalPlayer || GameManager.instance.isInGUI) {
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
                        { name = "Labyrinth", username = GameManager.instance.username });
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer ("TempLeaveGame")) {
                    GameManager.instance.LeaveMinigame();
                }
            }
        }
    }
}