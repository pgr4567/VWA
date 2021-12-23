namespace Menu.Settings {
    [System.Serializable]
    public struct SettingsList {
        public GraphicSettings graphicSettings;

        [System.Serializable]
        public struct GraphicSettings {
            public int screenWidth;
            public int screenHeight;
            public bool fullScreen;
            public int vsync;
        }
    }
}