using UnityEngine;

namespace Minigames.Labyrinth.Maze {
    [System.Serializable]
    public class MazeRoomSettings {
        public bool isRoom;
        public int weight;
        public int index;
        public GameObject[] walls;
        public GameObject randomWall {
            get {
                return walls[LabyrinthManager.random.Next(0, walls.Length)];
            }
        }
    }
}