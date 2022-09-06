using System;
using System.Text.Json.Serialization;

namespace BleConnector.Models {
    /// <summary>
    /// This class represents the defined results model provided by 'Beurer Gl50 evo' device documentation
    /// </summary>
    /// <example>
    /// JsonSerializer.Serialize(GlucometerMeasurement.ParseBytes(new byte[] { 22, 9, 0, 230, 7, 8, 5, 10, 10, 0, 55, 192, 17 }))
    /// </example>
    class GlucometerMeasurement {
        [JsonIgnore]
        public byte Flags { get; set; }
        public short SequenceNumber { get; set; }
        public Timestamp Timestamp { get; set; }
        public double Glucose { get; set; }
        public string Unit { get; set; }

        /// <summary>
        /// Gets the bytes received by glucometer device and returns a valid measurement
        /// </summary>
        /// <param name="data"></param>
        /// <returns>a valid glucometer measurement</returns>
        public static GlucometerMeasurement ParseBytes(byte[] data) {
            if (data.Length < 11) // Minimum required length of the array
                return null;

            return new GlucometerMeasurement {
                Flags = data[0],
                SequenceNumber = BitConverter.ToInt16(new byte[] { data[1], data[2] }, 0),
                Timestamp = new Timestamp {
                    Year = BitConverter.ToInt16(new byte[] { data[3], data[4] }, 0),
                    Month = data[5],
                    Day = data[6],
                    Hour = data[7],
                    Minute = data[8],
                    Second = data[9],
                },
                Glucose = (double)(data[10]) / 10.0,
                Unit = "mmol/L"
            };
        }
    }
}
