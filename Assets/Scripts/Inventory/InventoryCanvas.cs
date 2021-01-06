using System.Collections.Generic;
using System.Linq;
using General;
using Player;
using Shops;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
    public class InventoryCanvas : MonoBehaviour {
        [SerializeField] private GameObject inventory;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Transform itemParent;
        private readonly Dictionary<Item, GameObject> _items = new Dictionary<Item, GameObject> ();

        public void ToggleInventory () {
            if (GameManager.instance.isInGUI && !inventory.activeSelf || GameManager.instance.isServer) {
                return;
            }

            inventory.SetActive (!inventory.activeSelf);
            if (inventory.activeSelf) {
                GameManager.instance.isInGUI = true;
                MouseController.instance.ShowCursor ();

                LoadItems ();
            } else {
                GameManager.instance.isInGUI = false;
                MouseController.instance.HideCursor ();
            }
        }

        private void Update () {
            //TODO: USE NEW INPUT SYSTEM
            if (Input.GetKeyDown (KeyCode.I)) {
                ToggleInventory ();
            }
        }

        private void LoadItems () {
            foreach (Item item in Inventory.items.Where (item => !_items.ContainsKey(item))) {
                AddItem (item);
            }
        }

        private void AddItem (Item item) {
            GameObject go = Instantiate (itemPrefab, itemParent);
            go.GetComponentInChildren<Image> ().sprite = item.sprite;
            _items.Add (item, go);
        }
    }
}