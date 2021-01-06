using System.Collections.Generic;
using UnityEngine;

namespace Shops {
    public static class ShopManager {
        static ShopManager () {
            LoadItems ();
        }

        public static readonly List<Item> items = new List<Item> ();

        public static void LoadItems () {
            items.Clear ();
            items.AddRange (Resources.LoadAll<Item> ("Items"));
        }
    }
}