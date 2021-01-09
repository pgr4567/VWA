namespace Menu.Settings {
    public struct SettingsList {
        public GraphicSettings graphicSettings;

        public struct GraphicSettings {
            public int screenWidth;
            public int screenHeight;
            public bool fullScreen;
            public int vsync;
        }
    }
}