using UnityEngine;

namespace Shops {
    [CreateAssetMenu (fileName = "New Item", menuName = "Custom/New Item")]
    public class Item : ScriptableObject {
        public new string name;
        public Sprite sprite;
        public int price;
        public bool sellable = true;
        public ItemCategory category;
    }
}