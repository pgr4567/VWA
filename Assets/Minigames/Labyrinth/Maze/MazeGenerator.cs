using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Util;

namespace Minigames.Labyrinth.Maze {
    public class MazeGenerator : MonoBehaviour {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject passagePrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject pillarPrefab;

        [Range(0f, 1f)]
        public float doorProbability;
        [SerializeField] private MazeRoomSettings[] roomSettings;
        private MazeCell[,] cells;
        private IntVector2 size;
        private Vector2 startPos;
        private List<MazeRoom> rooms = new List<MazeRoom>();

        public IntVector2 RandomCoordinates {
            get {
                return new IntVector2(LabyrinthManager.random.Next(0, size.x), LabyrinthManager.random.Next(0, size.y));
            }
        }
        public bool ContainsCoordinates (IntVector2 coordinate) {
            return coordinate.x >= 0 && coordinate.x < size.x && coordinate.y >= 0 && coordinate.y < size.y;
        }
        public MazeCell GetCell (IntVector2 coordinates) {
            return cells[coordinates.x, coordinates.y];
        }

        public void Generate (IntVector2 size) {
            this.size = size;
            cells = new MazeCell[size.x, size.y];
            List<MazeCell> activeCells = new List<MazeCell>();
            DoFirstGenerationStep(activeCells);
            while (activeCells.Count > 0) {
                DoNextGenerationStep(activeCells);
            }
            GeneratePillars();
        }

        private void GeneratePillars() {
            List<Vector2> pillars = new List<Vector2>();
            for (int x = 0; x < size.x; x++) {
                for (int z = 0; z < size.x; z++) {
                    MazeCell cell = GetCell(new IntVector2(x, z));
                    if (!(cell.GetEdge(MazeDirection.North) is MazePassage) || cell.GetEdge(MazeDirection.North) is MazeDoor) {
                        if (!(cell.GetEdge(MazeDirection.East) is MazePassage) || cell.GetEdge(MazeDirection.East) is MazeDoor) {
                            Vector2 v2 = cell.coordinates.ToVector2() + MazeDirection.East.ToIntVector2().ToVector2() / 2 + MazeDirection.North.ToIntVector2().ToVector2() / 2;
                            if (!pillars.Contains(v2)) {
                                pillars.Add(v2);
                            }
                        }
                        if (!(cell.GetEdge(MazeDirection.West) is MazePassage) || cell.GetEdge(MazeDirection.West) is MazeDoor) {
                            Vector2 v2 = cell.coordinates.ToVector2() + MazeDirection.West.ToIntVector2().ToVector2() / 2 + MazeDirection.North.ToIntVector2().ToVector2() / 2;
                            if (!pillars.Contains(v2)) {
                                pillars.Add(v2);
                            }
                        }
                    }
                    if (!(cell.GetEdge(MazeDirection.South) is MazePassage) || cell.GetEdge(MazeDirection.South) is MazeDoor) {
                        if (!(cell.GetEdge(MazeDirection.East) is MazePassage) || cell.GetEdge(MazeDirection.East) is MazeDoor) {
                            Vector2 v2 = cell.coordinates.ToVector2() + MazeDirection.East.ToIntVector2().ToVector2() / 2 + MazeDirection.South.ToIntVector2().ToVector2() / 2;
                            if (!pillars.Contains(v2)) {
                                pillars.Add(v2);
                            }
                        }
                        if (!(cell.GetEdge(MazeDirection.West) is MazePassage) || cell.GetEdge(MazeDirection.West) is MazeDoor) {
                            Vector2 v2 = cell.coordinates.ToVector2() + MazeDirection.West.ToIntVector2().ToVector2() / 2 + MazeDirection.South.ToIntVector2().ToVector2() / 2;
                            if (!pillars.Contains(v2)) {
                                pillars.Add(v2);
                            }
                        }
                    }
                }
            }
            foreach (Vector2 v2 in pillars) {
                Instantiate(pillarPrefab, new Vector3(v2.x, 1f, v2.y) + transform.position + new Vector3((size.x - 1) / -2, 0, (size.y - 1) / -2) + new Vector3(-0.5f, 0f, -0.5f), Quaternion.identity, transform);
            }
        }

        private void DoFirstGenerationStep (List<MazeCell> activeCells) {
            IntVector2 coords = RandomCoordinates;
            MazeCell newCell = CreateCell(coords);
            newCell.Initialize(CreateRoom(-1));
            activeCells.Add(newCell);
            startPos = new Vector2(coords.x, coords.y);
        }

        private IntVector2 DoNextGenerationStep (List<MazeCell> activeCells) {
            int currentIndex = activeCells.Count - 1;
            MazeCell currentCell = activeCells[currentIndex];
            if (currentCell.IsFullyInitialized) {
                activeCells.RemoveAt(currentIndex);
                return currentCell.coordinates;
            }
            MazeDirection direction = currentCell.RandomUninitializedDirection;
            IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
            if (ContainsCoordinates(coordinates)) {
                MazeCell neighbour = cells[coordinates.x, coordinates.y];
                if (neighbour == null) {
                    neighbour = CreateCell(coordinates);
                    CreatePassage(currentCell, neighbour, direction);
                    activeCells.Add(neighbour);
                }
                else if (currentCell.room.settingsIndex == neighbour.room.settingsIndex && neighbour.room.settings.isRoom) {
                    CreatePassageInSameRoom(currentCell, neighbour, direction);
                }
                else {
                    CreateWall(currentCell, neighbour, direction);
                }
            }
            else {
                CreateWall(currentCell, null, direction);
            }
            return currentCell.coordinates;
        }

        private MazeCell CreateCell (IntVector2 coordinates) {
            MazeCell newCell = Instantiate(cellPrefab).GetComponent<MazeCell>();
            cells[coordinates.x, coordinates.y] = newCell;
            newCell.coordinates = coordinates;
            newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.y;
            newCell.transform.parent = transform;
            newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.y - size.y * 0.5f + 0.5f);
            return newCell;
        }

        private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
            GameObject prefab = LabyrinthManager.random.NextDouble() < doorProbability ? doorPrefab : passagePrefab;
            MazePassage passage = Instantiate(prefab).GetComponent<MazePassage>();
            passage.Initialize(cell, otherCell, direction);
            passage = Instantiate(prefab).GetComponent<MazePassage>();
            if (passage is MazeDoor) {
                otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
            }
            else {
                otherCell.Initialize(cell.room);
            }
            passage.Initialize(otherCell, cell, direction.GetOpposite());
        }

        private void CreatePassageInSameRoom (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
            MazePassage passage = Instantiate(passagePrefab).GetComponent<MazePassage>();
            passage.Initialize(cell, otherCell, direction);
            passage = Instantiate(passagePrefab).GetComponent<MazePassage>();
            passage.Initialize(otherCell, cell, direction.GetOpposite());
            if (cell.room != otherCell.room) {
                MazeRoom roomToAssimilate = otherCell.room;
                cell.room.Assimilate(roomToAssimilate);
                rooms.Remove(roomToAssimilate);
                Destroy(roomToAssimilate);
            }
        }

        private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
            MazeWall wall = Instantiate(cell.room.settings.randomWall).GetComponent<MazeWall>();
            wall.Initialize(cell, otherCell, direction);
            if (otherCell != null) {
                wall = Instantiate(otherCell.room.settings.randomWall).GetComponent<MazeWall>();
                wall.Initialize(otherCell, cell, direction.GetOpposite());
            }
        }

        private MazeRoom CreateRoom (int indexToExclude) {
            MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
            newRoom.settingsIndex = GetSettingsIndexFromWeight(LabyrinthManager.random.Next(0, roomSettings.Sum(r => r.weight)));
            if (newRoom.settingsIndex == indexToExclude) {
                newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
            }
            newRoom.settings = roomSettings[newRoom.settingsIndex];
            rooms.Add(newRoom);
            return newRoom;
        }
        private int GetSettingsIndexFromWeight(int weight) {
            List<MazeRoomSettings> visited = new List<MazeRoomSettings>();
            foreach (var rs in roomSettings.OrderBy(r => r.weight)) {
                if (weight - visited.Sum(r => r.weight) <= rs.weight) {
                    return rs.index;
                }
                visited.Add(rs);
            }
            return -1;
        }
    }
}