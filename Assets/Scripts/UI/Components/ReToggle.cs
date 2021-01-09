using UnityEngine;
using UnityEngine.UI;

namespace UI.Components {
    public class ReToggle : MonoBehaviour {
        [SerializeField] private Image toggle;
        public void ChangedToggle (bool isToggled) {
            toggle.color = isToggled ? Color.green : Color.red;
        }
    }
}