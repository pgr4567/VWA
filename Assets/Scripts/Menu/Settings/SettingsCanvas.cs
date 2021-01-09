using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace Menu.Settings {
    public class SettingsCanvas : MonoBehaviour {
        [SerializeField] private Transform tabParent;
        [SerializeField] private GameObject settingsGO;

        [SerializeField] private List<SettingsEntry> settings = new List<SettingsEntry> ();

        private GameObject _currentTab;

        private void Update () {
            if (Input.GetKeyDown (KeyCode.L) && !settingsGO.activeSelf) {
                Show ();
            }
        }

        private void Show () {
            Load ();
            _currentTab = tabParent.GetChild (0).gameObject;
            settingsGO.SetActive (true);
        }

        public void ShowTab (GameObject tab) {
            _currentTab.SetActive (false);
            tab.SetActive (true);
            _currentTab = tab;
        }

        public void Close () {
            _currentTab.SetActive (false);
            tabParent.GetChild (0).gameObject.SetActive (true);
            _currentTab = tabParent.GetChild (0).gameObject;
            settingsGO.SetActive (false);
        }

        public void Save () {
            SettingsList sList = new SettingsList { graphicSettings = Graphics.GetGraphicSettings (settings) };
            SettingManager.SaveSettings (sList);
            Close ();
        }

        private void Load () { Graphics.SetGraphicSettings (settings, SettingManager.settingsList.graphicSettings); }

        public static class Graphics {
            public static SettingsList.GraphicSettings GetGraphicSettings (List<SettingsEntry> settings) {
                TMP_Dropdown screenSize = settings.First (s => s.id == "screenSize").component as TMP_Dropdown;
                Toggle       fullScreen = settings.First (s => s.id == "fullScreen").component as Toggle;
                TMP_Dropdown vsync      = settings.First (s => s.id == "vsync").component as TMP_Dropdown;

                Debug.Assert (screenSize != null, nameof(screenSize) + " != null");
                Debug.Assert (fullScreen != null, nameof(fullScreen) + " != null");
                Debug.Assert (vsync != null, nameof(vsync) + " != null");

                return new SettingsList.GraphicSettings {
                    screenWidth =
                        GetWidthFor (screenSize.options[screenSize.value]),
                    screenHeight =
                        GetHeightFor (screenSize.options[screenSize.value]),
                    fullScreen = fullScreen.isOn,
                    vsync      = vsync.value
                };
            }

            public static void SetGraphicSettings (List<SettingsEntry> settings,
                SettingsList.GraphicSettings graphics) {
                TMP_Dropdown screenSize = settings.First (s => s.id == "screenSize").component as TMP_Dropdown;
                Toggle       fullScreen = settings.First (s => s.id == "fullScreen").component as Toggle;
                TMP_Dropdown vsync      = settings.First (s => s.id == "vsync").component as TMP_Dropdown;

                Debug.Assert (screenSize != null, nameof(screenSize) + " != null");
                Debug.Assert (fullScreen != null, nameof(fullScreen) + " != null");
                Debug.Assert (vsync != null, nameof(vsync) + " != null");

                screenSize.value = screenSize.options.FindIndex (o =>
                    o.text.Equals (graphics.screenWidth + "x" + graphics.screenHeight));
                fullScreen.isOn = graphics.fullScreen;
                vsync.value     = graphics.vsync;
            }

            private static int GetWidthFor (TMP_Dropdown.OptionData value) {
                return int.Parse (value.text.Split ('x')[0]);
            }

            private static int GetHeightFor (TMP_Dropdown.OptionData value) {
                return int.Parse (value.text.Split ('x')[1]);
            }
        }
    }

    [Serializable]
    public struct SettingsEntry {
        public string id;
        public MonoBehaviour component;
    }
}