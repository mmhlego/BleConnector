using System.Text.Json;

namespace BleConnector.Models {
    static class Settings {
        // Scan time out (to small and too large is not good)
        public static int ScanTimeout = 10;
        // Approximate audio length
        public static int AudioLength = 10;
        // Set to true if you want the weight scale to print live measurement
        public static bool UpdateWeight = false;
        // When exporting, by changing this value to false all other logs will be invisible
        public static bool Debug = true;

        public static void loadSettings() {
            try {
                string data = File.ReadAllText("./settings.json");
                SettingsModel? settings = JsonSerializer.Deserialize<SettingsModel>(data);

                if (settings != null) {
                    ScanTimeout = settings.ScanTimeout;
                    AudioLength = settings.AudioLength;
                    UpdateWeight = settings.UpdateWeight;
                    Debug = settings.Debug;
                }
            } catch (Exception ex) {
            }
        }
    }

    /// <summary>
    /// This model is used to load the settings from "settings.json" file
    /// </summary>
    class SettingsModel {
        public int ScanTimeout { get; set; }
        public int AudioLength { get; set; }
        public bool UpdateWeight { get; set; }
        public bool Debug { get; set; }
    }
}
