/// <summary>
/// This class represents the defined results model provided by 'Beurer FT95 Thermometer' device documentation
/// </summary>
namespace BleConnector.Models {
    class Thermometer {
        public ThermometerFlags Flags { get; set; }
        public float Temperature { get; set; }
        public Timestamp Timestamp { get; set; }
        public string Type { get; set; }
    }

    class ThermometerFlags {
        public string Unit { get; set; }
        public bool TimeStampFlag { get; set; }
        public bool TypeFlag { get; set; }
        public bool HasFever { get; set; }
    }
}
