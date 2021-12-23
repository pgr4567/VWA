using System;
using System.Linq;
using General;
using Minigames.Labyrinth.Maze;
using Mirror;
using Networking;
using TMPro;
using UnityEngine;

namespace Minigames.Labyrinth {
    public class LabyrinthManager : MinigameManager {
        [SerializeField] private GameObject ui;
        [SerializeField] private TMP_Text timer;
        [SerializeField] private GameObject mazePrefab;
        [SerializeField] private GameObject spawnPosGO;
        [SerializeField] private GameObject teamEnd0;
        [SerializeField] private GameObject teamEnd1;
        private float _current;
        private float _time;
        private MazeGenerator maze;
        private Util.IntVector2 mazeSize = new Util.IntVector2 (10, 10);
        private float mazeDistance = 10f;
        private int winnerMoney = 10;

        public static System.Random random { get; private set; }

        public override void StartGameServer () {
            base.StartGameServer ();

            _time = Time.time;
        }

        public override void StartGameClient () {
            base.StartGameClient ();

            ui.SetActive (true);
            _time = Time.time;
        }

        public override void PrepareGameServer () {
            ServerReady = true;
            string seed = GenerateRandomSeed (9);
            System.Random random = new System.Random (int.Parse (seed));
            Vector2 spawnPos = new Vector2 (random.Next (0, mazeSize.x), random.Next (0, mazeSize.y));
            teamSpawnPositions.Add (Instantiate (spawnPosGO, new Vector3 (spawnPos.x, 0, spawnPos.y) + transform.position + new Vector3 (-0.5f, 1f, -0.5f) + new Vector3 ((mazeSize.x - 1) / -2, 0, (mazeSize.y - 1) / -2), Quaternion.identity, transform).transform);
            teamSpawnPositions.Add (Instantiate (spawnPosGO, new Vector3 (spawnPos.x, mazeDistance, spawnPos.y) + transform.position + new Vector3 (-0.5f, 1f, -0.5f) + new Vector3 ((mazeSize.x - 1) / -2, 0, (mazeSize.y - 1) / -2), Quaternion.identity, transform).transform);
            Vector2 end = new Vector2 (Mathf.Infinity, Mathf.Infinity);
            while ((end - spawnPos).magnitude > 4f) {
                end = new Vector2 (UnityEngine.Random.Range (0, mazeSize.x), UnityEngine.Random.Range (0, mazeSize.y));
            }
            Instantiate (teamEnd0, new Vector3 (end.x, 0, end.y) + transform.position + new Vector3 (-0.5f, 1f, -0.5f) + new Vector3 ((mazeSize.x - 1) / -2, 0, (mazeSize.y - 1) / -2), Quaternion.identity, transform).GetComponent<LabyrinthGoal> ().manager = this;
            Instantiate (teamEnd1, new Vector3 (end.x, mazeDistance, end.y) + transform.position + new Vector3 (-0.5f, 1f, -0.5f) + new Vector3 ((mazeSize.x - 1) / -2, 0, (mazeSize.y - 1) / -2), Quaternion.identity, transform).GetComponent<LabyrinthGoal> ().manager = this;

            foreach (string username in players) {
                GameManager.instance.TargetChangePlayerSpeed (MainNetworkManager.instance.players[username], 3f, 3f);
            }

            PrepareMinigameMessage msg = new PrepareMinigameMessage () { gameID = gameID, args = new string[] { seed, end.x.ToString (), end.y.ToString () } };
            if (GameManager.instance.DEBUG) {
                GenerateMaze (msg, true);
            }
            NetworkServer.SendToAll (msg);
        }

        private string GenerateRandomSeed (int length) {
            string seed = UnityEngine.Random.Range (1, 10).ToString ();
            for (int i = 0; i < length - 1; i++) {
                seed += UnityEngine.Random.Range (0, 10).ToString ();
            }
            return seed;
        }

        private void GenerateMaze (PrepareMinigameMessage msg, bool server = false) {
            random = new System.Random (int.Parse (msg.args[0]));
            Vector2 end = new Vector2 (int.Parse (msg.args[1]), int.Parse (msg.args[2]));

            if (server) {
                maze = Instantiate (mazePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<MazeGenerator> ();
                maze.transform.localPosition = Vector3.zero;
                maze.Generate (mazeSize);
                maze = Instantiate (maze, Vector3.zero, Quaternion.identity, transform).GetComponent<MazeGenerator> ();
                maze.transform.localPosition = new Vector3 (0f, mazeDistance, 0f);
            } else {
                int team = GameManager.instance.currentTeam;
                maze = Instantiate (mazePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<MazeGenerator> ();
                maze.transform.localPosition = team == 0 ? Vector3.zero : new Vector3 (0f, mazeDistance, 0f);
                maze.Generate (mazeSize);

                if (team == 0) {
                    Instantiate (teamEnd0, new Vector3 (end.x, 0, end.y) + transform.position + new Vector3 (-0.5f, 1f, -0.5f) + new Vector3 ((mazeSize.x - 1) / -2, 0, (mazeSize.y - 1) / -2), Quaternion.identity, transform).GetComponent<LabyrinthGoal> ().manager = this;
                } else {
                    Instantiate (teamEnd1, new Vector3 (end.x, mazeDistance, end.y) + transform.position + new Vector3 (-0.5f, 1f, -0.5f) + new Vector3 ((mazeSize.x - 1) / -2, 0, (mazeSize.y - 1) / -2), Quaternion.identity, transform).GetComponent<LabyrinthGoal> ().manager = this;
                }
            }
        }

        public override void PrepareGameClient (PrepareMinigameMessage msg) {
            GenerateMaze (msg);

            base.PrepareGameClient (msg);
        }

        protected override void GameLoopServer () {
            _current = Time.time - _time;
            if (teams.Values.Count (team => team.points >= minigame.winningPoints) > 0) {
                MinigameTeam winner = teams.Values.First (team => team.points >= minigame.winningPoints);
                FinishGame (winner);
                foreach (string user in winner.players) {
                    Helpers.Get ("http://vwaspiel.de:3001/addMoney?username=" + user + "&amount=" + winnerMoney);
                }
            }
        }

        protected override void GameLoopClient () {
            _current = Time.time - _time;
            timer.text = _current > 60f ? Math.Round (_current / 60f, 2) + "min" : Math.Round (_current, 1) + "s";
        }
    }
}