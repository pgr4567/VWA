using UnityEngine;
using Util;

namespace Minigames.Labyrinth.Maze {
    public class MazeCell : MonoBehaviour {
        public IntVector2 coordinates;
        public MazeRoom room;
        private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.Count];
        private int initializedEdgeCount;
        public bool IsFullyInitialized {
            get {
                return initializedEdgeCount == MazeDirections.Count;
            }
        }

        public MazeDirection RandomUninitializedDirection {
            get {
                int skips = LabyrinthManager.random.Next(0, MazeDirections.Count - initializedEdgeCount);
                for (int i = 0; i < MazeDirections.Count; i++) {
                    if (edges[i] == null) {
                        if (skips == 0) {
                            return (MazeDirection)i;
                        }
                        skips -= 1;
                    }
                }
                throw new System.InvalidOperationException("MazeCell has no uninitialized directions left.");
            }
        }

        public void Initialize (MazeRoom room) {
            room.Add(this);
        }

        public MazeCellEdge GetEdge (MazeDirection direction) {
            return edges[(int)direction];
        }

        public void SetEdge (MazeDirection direction, MazeCellEdge edge) {
            edges[(int)direction] = edge;
            initializedEdgeCount += 1;
        }
    }
}