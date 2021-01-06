﻿using System.Threading.Tasks;
using General;
using UnityEngine;

namespace Player {
    public class MouseController : MonoBehaviour {
        public static MouseController instance;

        private async void Awake () {
            //TODO: WHYYYYYYY
            await Task.Delay (200);
            if (GameManager.instance.isServer) {
                return;
            }
            if (instance != null) {
                Debug.LogWarning ("There must only be one MouseController in the scene.");
                Destroy (this);
            }

            instance = this;
        }

        private void Update () {
            if (Input.GetKeyDown (KeyCode.Escape)) {
                ShowCursor ();
            } else if (Input.GetKeyUp (KeyCode.Escape) && !GameManager.instance.isInGUI) {
                HideCursor ();
            }
        }

        private void OnEnable () { HideCursor (); }

        public void ShowCursor () { Cursor.lockState = CursorLockMode.None; }

        public void HideCursor () { Cursor.lockState = CursorLockMode.Locked; }
    }
}