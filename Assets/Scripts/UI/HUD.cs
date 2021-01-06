using System.Threading.Tasks;
using General;
using Networking;
using Networking.RequestMessages;
using TMPro;
using UnityEngine;

namespace UI {
    public class HUD : MonoBehaviour {
        [SerializeField] private TMP_Text moneyText;
        public int money { get; private set; }
        public static HUD instance;

        private void Awake () {
            if (instance != null) {
                Debug.LogWarning ("There should only ever be one HUD.");
                Destroy (gameObject);
                return;
            }

            instance = this;
        }

        public void LeaveGame () { MainNetworkManager.instance.StopClient (); }

        private async void OnEnable () {
            //TODO: WHYYYYYYY
            await Task.Delay (200);
            if (GameManager.instance.isServer) {
                return;
            }

            SetupRequestMoney ();
        }

        private async void SetupRequestMoney () {
            //TODO: WHYYYYYYY
            await Task.Delay (1000);
            RequestManagerClient.instance.onResponse += (req, res, args) => {
                if (req == "money") {
                    OnMoneyChanged (int.Parse (res.Substring (1)));
                }
            };
            RequestManagerClient.instance.SendRequest ("money");
        }

        public void OnMoneyChanged (int money) {
            this.money     = money;
            moneyText.text = "$" + money;
        }
    }
}