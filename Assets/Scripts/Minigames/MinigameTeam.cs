using System.Collections.Generic;
using UnityEngine;

namespace Minigames {
    public class MinigameTeam {
        public List<string> players = new List<string> ();
        public int points = 0;
        public Vector3 spawnPoint = Vector3.zero;
    }
}