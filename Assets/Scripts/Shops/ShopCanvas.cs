using System.Linq;
using System.Threading.Tasks;
using General;
using Networking.RequestMessages;
using Player;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Shops {
    public class ShopCanvas : MonoBehaviour {
        [SerializeField] private GameObject shop;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Transform itemParent;

        private async void Start () {
            //TODO: WHYYYYYYY
            await Task.Delay (200);
            if (GameManager.instance.isServer) {
                return;
            }
            RequestManagerClient.instance.onResponse += (req, item, args) => {
                if (req == "trybuy") {
                    Buy (args[0]);
                }
            };
            foreach (Item item in ShopManager.items.Where(i => i.sellable && !Inventory.Inventory.items.Contains(i))) {
                AddItem (item);
            }
        }

        private void Update () {
            //TODO: USE NEW INPUT SYSTEM
            if (Input.GetKeyDown (KeyCode.H)) {
                ToggleShop ();
            }
        }

        public void AddItem (Item item) {
            GameObject go = Instantiate (itemPrefab, itemParent);
            go.GetComponentInChildren<Image> ().sprite  = item.sprite;
            go.GetComponentsInChildren<TMP_Text> ()[0].text = "$" + item.price;
            go.GetComponentInChildren<Button>().onClick.AddListener(() => {
                TryBuy (item.name);
            });
        }

        public void TryBuy (string item) {
            RequestManagerClient.instance.SendRequest("trybuy", item);
        }

        private void Buy (string item) {
            HUD.instance.OnMoneyChanged(HUD.instance.money - ShopManager.items.First(i => i.name == item).price);
            Inventory.Inventory.AddItem(ShopManager.items.First(i => i.name == item));
        }

        public void ToggleShop () { 
            if (GameManager.instance.isInGUI && !shop.activeSelf || GameManager.instance.isServer) {
                return;
            }
            shop.SetActive(!shop.activeSelf);
            if (shop.activeSelf) {
                GameManager.instance.isInGUI = true;
                MouseController.instance.ShowCursor();
            } else {
                GameManager.instance.isInGUI = false;
                MouseController.instance.HideCursor();
            }
        }
    }
}