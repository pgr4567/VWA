using System.Collections.Generic;
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
        private readonly Dictionary<Item, GameObject> _items = new Dictionary<Item, GameObject> ();

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
            ReloadShop ();
        }

        private void ReloadShop () {
            foreach (Item item in ShopManager.items.Where (i => i.sellable && !Inventory.Inventory.items.Contains (i) && !_items.ContainsKey (i))) {
                AddItem (item);
            }

            foreach (Item item in Inventory.Inventory.items) {
                RemoveItem (item);
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
            go.GetComponentInChildren<Image> ().sprite = item.sprite;
            go.GetComponentsInChildren<TMP_Text> ()[0].text = "$" + item.price;
            go.GetComponentInChildren<Button> ().onClick.AddListener (() => { TryBuy (item.name); });
            _items.Add (item, go);
        }

        public void RemoveItem (Item item) {
            if (!_items.ContainsKey (item)) {
                return;
            }
            Destroy (_items[item]);
            _items.Remove (item);
        }

        public void TryBuy (string item) {
            if (HUD.instance.money >= ShopManager.items.First (i => i.name == item).price) {
                RequestManagerClient.instance.SendRequest ("trybuy", item);
            }
        }

        private void Buy (string item) {
            HUD.instance.OnMoneyChanged (HUD.instance.money - ShopManager.items.First (i => i.name == item).price);
            Inventory.Inventory.AddItem (ShopManager.items.First (i => i.name == item));
            RemoveItem (ShopManager.items.First (i => i.name == item));
        }

        public void ToggleShop () {
            if (GameManager.instance.isInGUI && !shop.activeSelf || GameManager.instance.isServer) {
                return;
            }

            shop.SetActive (!shop.activeSelf);
            if (shop.activeSelf) {
                GameManager.instance.isInGUI = true;
                MouseController.instance.ShowCursor ();

                ReloadShop ();
            } else {
                GameManager.instance.isInGUI = false;
                MouseController.instance.HideCursor ();
            }
        }
    }
}