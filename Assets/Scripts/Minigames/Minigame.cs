using UnityEngine;

namespace Minigames {
    [CreateAssetMenu (fileName = "New Minigame", menuName = "Custom/New Minigame")]
    public class Minigame : ScriptableObject {
        public new string name;
        public string description;
        public string stats;
        public GameObject world;
        public int maxInstances;
        public int requiredTeams;
        public int maxTeams;
        public int requiredPlayersPerTeam;
        public int maxPlayersPerTeam;
        public int winningPoints;
    }
}