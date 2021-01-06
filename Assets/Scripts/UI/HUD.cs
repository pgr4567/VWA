using System.Threading.Tasks;
using General;
using Mirror;
using Networking;
using Networking.RequestMessages;
using TMPro;
using UnityEngine;

namespace UI {
    public class HUD : MonoBehaviour {
        [SerializeField] private TMP_Text moneyText;
        public void LeaveGame () {
            MainNetworkManager.instance.StopClient();
        }

        private async void OnEnable () {
            //TODO: WHYYYYYYY
            await Task.Delay (200);
            if (GameManager.instance.isServer) {
                return;
            }
            SetupRequestMoney();
        }

        private async void SetupRequestMoney () {
            //TODO: WHYYYYYYY
            await Task.Delay (1000);
            RequestManagerClient.instance.onMoneyRequest += OnMoneyChanged;
            RequestManagerClient.instance.SendRequest("money");
        }

        private void OnMoneyChanged (string money) {
            moneyText.text = money;
        }
    }
}