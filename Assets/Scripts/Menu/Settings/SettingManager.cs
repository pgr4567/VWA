using System.IO;
using UnityEngine;

namespace Menu.Settings {
    public static class SettingManager {
        private static readonly string saveFile = "settings.vwa";

        static SettingManager () {
            LoadSettings();
        }
        
        public static string savePath {
            get { return Path.Combine (Application.persistentDataPath, saveFile); }
        }

        public static SettingsList settingsList { get; private set; }

        public static void LoadSettings () {
            if (!File.Exists (savePath)) {
                LoadDefaults ();
                return;
            }
            settingsList = JsonUtility.FromJson<SettingsList> (File.ReadAllText (savePath));

            ApplySettings ();
        }

        private static void LoadDefaults () {
            settingsList = new SettingsList {
                graphicSettings = new SettingsList.GraphicSettings {
                    screenWidth  = 1920,
                    screenHeight = 1080,
                    fullScreen   = true,
                    vsync        = 0
                }
            };
            SaveSettings (settingsList);
        }

        public static void SaveSettings (SettingsList settings) {
            settingsList = settings;
            ApplySettings ();

            File.WriteAllText (savePath, JsonUtility.ToJson (settingsList));
        }

        private static void ApplySettings () {
            SettingsList.GraphicSettings graphicSettings = settingsList.graphicSettings;
            QualitySettings.vSyncCount = graphicSettings.vsync;
            Screen.SetResolution (graphicSettings.screenWidth, graphicSettings.screenHeight,
                graphicSettings.fullScreen);
        }
    }
}