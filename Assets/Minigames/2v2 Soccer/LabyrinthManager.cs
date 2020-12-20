using TMPro;
using UnityEngine;

public class LabyrinthManager : MinigameManager {
    private float time = 0f;
    private float current = 0f;
    [SerializeField] private GameObject ui;
    [SerializeField] private TMP_Text timer;
    public override void StartGameServer () {
        base.StartGameServer ();

        time = Time.time;
    }
    public override void StartGameClient () {
        base.StartGameClient ();

        ui.SetActive (true);
        time = Time.time;
    }
    protected override void GameLoopServer () {
        current = Time.time - time;
        foreach (MinigameTeam team in teams.Values) {
            if (team.points >= minigame.winningPoints) {
                isRunning = false;
                Debug.Log ($"NICE! That took {current}s.");

                MinigameDispatcher.instance.RemoveMinigameLobby (minigame.name, number, gameID);
            }
        }
    }
    protected override void GameLoopClient () {
        current = Time.time - time;
        timer.text = current > 60f ? System.Math.Round (current / 60f, 2) + "min" : System.Math.Round (current, 1) + "s";
    }
}