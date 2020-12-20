using UnityEngine;

public class LabyrinthGoal : MonoBehaviour {
    [SerializeField] private LabyrinthManager manager;
    [SerializeField] private int teamNumber = -1;
    public void OnTriggerEnter (Collider coll) {
        if (coll.tag == "Player") {
            manager.ScoreTeam (1, teamNumber);
        }
    }
}