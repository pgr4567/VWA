using Networking;
using UnityEngine;

namespace UI {
    public class HUD : MonoBehaviour {
        public void LeaveGame () {
            MainNetworkManager.instance.StopClient();
        }
    }
}