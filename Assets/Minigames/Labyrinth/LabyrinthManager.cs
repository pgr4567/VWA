using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Minigames.Labyrinth {
    public class LabyrinthManager : MinigameManager {
        [SerializeField] private GameObject ui;
        [SerializeField] private TMP_Text timer;
        private float _current;
        private float _time;

        public override void StartGameServer () {
            base.StartGameServer ();

            _time = Time.time;
        }

        public override void StartGameClient () {
            base.StartGameClient ();

            ui.SetActive (true);
            _time = Time.time;
        }

        protected override void GameLoopServer () {
            _current = Time.time - _time;
            if (teams.Values.Count (team => team.points >= minigame.winningPoints) > 0) {
                FinishGame(teams.Values.First (team => team.points >= minigame.winningPoints));
            }
        }

        protected override void GameLoopClient () {
            _current   = Time.time - _time;
            timer.text = _current > 60f ? Math.Round (_current / 60f, 2) + "min" : Math.Round (_current, 1) + "s";
        }
    }
}