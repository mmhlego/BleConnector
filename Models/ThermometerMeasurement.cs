using System;

namespace BleConnector.Models {
    /// <summary>
    /// This class represents the defined results model provided by 'Beurer FT95 Thermometer' device documentation
    /// </summary>
    /// <example>
    /// JsonSerializer.Serialize(ThermometerMeasurement.ParseBytes(new byte[] { 7, 16, 3, 0, 255, 225, 7, 4, 13, 14, 7, 0, 255 }))
    /// </example>
    class ThermometerMeasurement {
        public ThermometerFlags Flags { get; set; }
        public float Temperature { get; set; }
        public Timestamp Timestamp { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Gets the bytes received by thermometer device and returns a valid measurement
        /// </summary>
        /// <param name="data"></param >
        /// <returns>a valid thermometer measurement</returns>
        public static ThermometerMeasurement ParseBytes(byte[] data) {
            if (data.Length != 13) // Length of a valid measurement
                return null;


            int temperature = BitConverter.ToInt16(new byte[] { data[1], data[2] }, 0);
            return new ThermometerMeasurement {
                Flags = new ThermometerFlags {
                    Unit = ((data[0] & 0x80) != 0) ? "Celsius" : "Fahrenheit",
                    TimestampFlag = (data[0] & 0x40) == 0,
                    TypeFlag = (data[0] & 0x20) == 0,
                    HasFever = (data[0] & 0x01) == 0,
                },
                Temperature = ((float)temperature) / 10,
                Timestamp = new Timestamp {
                    Year = BitConverter.ToInt16(new byte[] { data[5], data[6] }, 0),
                    Month = data[7],
                    Day = data[8],
                    Hour = data[9],
                    Minute = data[10],
                    Second = data[11],
                },
                Type = (data[2] == 0x2) ? "Body" : "Object"
            };
        }
    }

    class ThermometerFlags {
        public string Unit { get; set; }
        public bool TimestampFlag { get; set; }
        public bool TypeFlag { get; set; }
        public bool HasFever { get; set; }
    }
}
