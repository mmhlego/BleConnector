namespace BleConnector.Models {
    /// <summary>
    /// This class represents the defined results model provided by 'Beurer PO60 Glucometer' device documentation
    /// </summary>
    /// <example>
    /// JsonSerializer.Serialize(OximeterMeasurement.ParseBytes(new byte[] { 0xE9, 0x00, 0x0F, 0x0B, 0x06, 0x0D, 0x14, 0x25, 0x0F, 0x0B, 0x06, 0x0D, 0x14, 0x3A, 0x00, 0x00, 0x16, 0x60, 0x60, 0x60, 0x6F, 0x6E, 0x6E, 0x4B }))
    /// </example>
    class OximeterMeasurement {
        public byte Header { get; set; }
        public int Packetnumber { get; set; }
        public Timestamp Start { get; set; }
        public Timestamp End { get; set; }
        public Range<int> SpO2 { get; set; }
        public Range<int> PulseRate { get; set; }

        /// <summary>
        /// Gets the bytes received by oximeter device and returns a valid measurement
        /// </summary>
        /// <param name="data"></param>
        /// <returns>a valid oximeter measurement</returns>
        public static OximeterMeasurement ParseBytes(byte[] data) {
            if (data.Length != 24) // Length of a valid measurement
                return null;

            byte msbBits = data[14];

            return new OximeterMeasurement {
                Header = data[0],
                Packetnumber = data[1] & 0x0F,
                Start = new Timestamp {
                    Year = 2000 + data[2],
                    Month = data[3],
                    Day = data[4],
                    Hour = data[5],
                    Minute = data[6],
                    Second = data[7],
                },
                End = new Timestamp {
                    Year = 2000 + data[8],
                    Month = data[9],
                    Day = data[10],
                    Hour = data[11],
                    Minute = data[12],
                    Second = data[13],
                },
                SpO2 = new Range<int> {
                    Min = data[18],
                    Average = data[19],
                    Max = data[17],
                },
                PulseRate = new Range<int> {
                    Min = data[21] + (msbBits & 0x10) * 16,
                    Average = data[22] + (msbBits & 0x08) * 16,
                    Max = data[20] + (msbBits & 0x20) * 16,
                }
            };
        }
    }
}
