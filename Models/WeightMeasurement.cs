using System;

namespace BleConnector.Models {
    /// <summary>
    /// This class represents the defined results model provided by 'Beurer BF70 Weight Scale' device documentation
    /// </summary>
    /// <example>
    /// JsonSerializer.Serialize(WeightMeasurement.ParseBytes(new byte[] { 231, 88, 1, 1, 38 }))
    /// </example>
    class WeightMeasurement {
        public string Unit { get; set; } = "Kilograms";
        public double Weight { get; set; }

        public static WeightMeasurement ParseBytes(byte[] data) {
            if (data.Length != 5) // Length of a valid measurement
                return null;

            return new WeightMeasurement {
                Weight = Convert.ToSingle(data[3] * 256 + data[4]) / 20,
            };
        }
    }
}
