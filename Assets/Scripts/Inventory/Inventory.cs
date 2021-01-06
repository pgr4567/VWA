using System.Collections.Generic;
using System.Linq;
using Networking.RequestMessages;
using Shops;

namespace Inventory {
    public static class Inventory {
        public static readonly List<Item> items = new List<Item> ();
        static Inventory () {
            RequestManagerClient.instance.onResponse += (req, res, args) => {
                if (req == "getInventory") {
                    LoadItems (res);
                }
            };
            RequestManagerClient.instance.SendRequest ("getInventory");
        }

        private static void LoadItems (string itemStr) {
            foreach (string item in itemStr.Split (';')) {
                if (ShopManager.items.Count (i => i.name == item) > 0) {
                    AddItem (ShopManager.items.First(i => i.name == item));
                }
            }
        }
        public static void AddItem (Item item) {
            items.Add (item);
        }
    }
}