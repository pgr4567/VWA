using UnityEngine;

namespace Minigames.Labyrinth {
    public class LabyrinthGoal : MonoBehaviour {
        [SerializeField] private LabyrinthManager manager;
        [SerializeField] private int teamNumber = -1;

        public void OnTriggerEnter (Collider coll) {
            if (coll.CompareTag ("Player")) {
                manager.ScoreTeam (1, teamNumber);
            }
        }
    }
}