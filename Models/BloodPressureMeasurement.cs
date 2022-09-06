using System;

namespace BleConnector.Models {
    /// <summary>
    /// This class represents the defined results model provided by 'Beurer BM67 Blood Pressure' device documentation
    /// </summary>
    /// <example>
    /// JsonSerializer.Serialize(BloodPressureMeasurement.ParseBytes(new byte[] { 0x1E, 0x70, 0x00, 0x4D, 0x00, 0x00, 0x00, 0xDF, 0x07, 0x01, 0x0E, 0x0A, 0x37, 0x00, 0x48, 0x00, 0x01, 0x00, 0x00 }))
    /// </example>
    class BloodPressureMeasurement {
        public BloodPressureFlags Flags { get; set; }
        public float Systolic { get; set; }
        public float Diastolic { get; set; }
        public float MeanArterialPressure { get; set; }
        public Timestamp Timestamp { get; set; }
        public float PulseRate { get; set; }
        public int UserId { get; set; }
        public BloodPressureStatus Status { get; set; }

        /// <summary>
        /// Gets the bytes received by blood pressure device and returns a valid measurement
        /// </summary>
        /// <param name="data"></param >
        /// <returns>a valid blood pressure measurement</returns>
        public static BloodPressureMeasurement ParseBytes(byte[] data) {
            if (data.Length != 19) // Length of a valid measurement
                return null;

            return new BloodPressureMeasurement {
                Flags = BloodPressureFlags.ParseByte(data[0]),
                Systolic = Convert.ToSingle(BitConverter.ToInt16(new byte[] { data[1], data[2] }, 0)),
                Diastolic = Convert.ToSingle(BitConverter.ToInt16(new byte[] { data[3], data[4] }, 0)),
                MeanArterialPressure = Convert.ToSingle(BitConverter.ToInt16(new byte[] { data[5], data[6] }, 0)),
                Timestamp = new Timestamp {
                    Year = BitConverter.ToInt16(new byte[] { data[7], data[8] }, 0),
                    Month = data[9],
                    Day = data[10],
                    Hour = data[11],
                    Minute = data[12],
                    Second = data[13],
                },
                PulseRate = Convert.ToSingle(BitConverter.ToInt16(new byte[] { data[14], data[15] }, 0)),
                UserId = data[16] + 1,
                Status = BloodPressureStatus.ParseBytes(new byte[] { data[17], data[18] })
            };
        }
    }

    /// <summary>
    /// This class is used to hold blood pressure measurement flags inside itself
    /// </summary>
    class BloodPressureFlags {
        public string Unit { get; set; }
        public bool TimestampFlag { get; set; }
        public bool PulseRateFlag { get; set; }
        public bool UserIdFlag { get; set; }
        public bool MeasurementStatsuFlag { get; set; }

        public static BloodPressureFlags ParseByte(byte data) {
            return new BloodPressureFlags {
                Unit = (data & 0x80) == 0 ? "mmHg" : "kPa",
                TimestampFlag = (data & 0x40) == 0,
                PulseRateFlag = (data & 0x20) == 0,
                UserIdFlag = (data & 0x10) == 0,
                MeasurementStatsuFlag = (data & 0x08) == 0
            };
        }
    }

    /// <summary>
    /// This class is used to hold blood pressure measurement status and errors inside itself
    /// </summary>
    class BloodPressureStatus {
        public bool BodyMovement { get; set; }
        public string CuffFit { get; set; }
        public bool IrregularPulse { get; set; }
        public string PulseRateRange { get; set; }
        public string MeasurementPosition { get; set; }

        public static BloodPressureStatus ParseBytes(byte[] data) {
            return new BloodPressureStatus {
                BodyMovement = (data[0] & 0x80) != 0,
                CuffFit = (data[0] & 0x40) == 0 ? "Proper fit" : "Too loose",
                IrregularPulse = (data[0] & 0x20) != 0,
                PulseRateRange = (data[0] & 0x18) == 0x00 ? "In Range" : ((data[0] & 0x18) == 0x08 ? "Exceeds Upper Limit" : "Below Lower Limit"),
                MeasurementPosition = (data[0] & 0x04) == 0 ? "Proper Position" : "Improper Position"
            };
        }
    }
}
