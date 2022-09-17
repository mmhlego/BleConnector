using BleConnector.Models;
using System.Text.Json;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using BleConnector.Utils;
using mintti_sdk.ble;

namespace BleConnector.Ble {
    static class Manager {
        public static async Task<bool> Communicate(DeviceTypes deviceType) {
            switch (deviceType) {
                case DeviceTypes.Glucometer:
                    return await CommunicateWithGlucometer();
                case DeviceTypes.Oximeter:
                    return await CommunicateWithOximeter();
                case DeviceTypes.Thermometer:
                    return await CommunicateWithThermometer();
                case DeviceTypes.BloodPressure:
                    return await CommunicateWithBloodPressure();
                case DeviceTypes.WeightScale:
                    return await CommunicateWithWeightScale();
                case DeviceTypes.Stethoscope:
                    return await CommunicateWithStethoscope();
            }
            return false;
        }

        ///================================================================================================================= Thermometer Section

        /// <summary>
        /// Store latest thermometer measurement inside this variable
        /// </summary>
        static ThermometerMeasurement latestThermometerMeasuremment = null;

        /// <summary>
        /// Set of instructions to get thermometer measurements
        /// </summary>
        /// <example> Command: Thermometer ff:00:00:00:06:01 </example>
        private static async Task<bool> CommunicateWithThermometer() {
            string MeasurementCharacteristic = "00002a1c-0000-1000-8000-00805f9b34fb";
            if (!await Interface.Subscribe(MeasurementCharacteristic, ThermometerListener)) { return false; }

            await Task.Delay(10 * 1000);

            Console.WriteLine(JsonSerializer.Serialize(latestThermometerMeasuremment));

            Interface.Unsubscribe(MeasurementCharacteristic, ThermometerListener);

            return true;
        }
        static void ThermometerListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            latestThermometerMeasuremment = ThermometerMeasurement.ParseBytes(data);
        }

        ///================================================================================================================= Glucometer Section

        /// <summary>
        /// Set of instructions to get glucometer measurements
        /// </summary>
        /// <example> Command: Glucometer f7:4c:87:32:62:ff </example>
        private static async Task<bool> CommunicateWithGlucometer() {
            string AccessControlCharacteristic = "00002a52-0000-1000-8000-00805f9b34fb";
            string MeasurementCharacteristic = "00002a18-0000-1000-8000-00805f9b34fb";

            if (!await Interface.Subscribe(AccessControlCharacteristic, GlucometerListener)) { return false; }
            if (!await Interface.Subscribe(MeasurementCharacteristic, GlucometerListener)) { return false; }

            await Interface.WriteData(AccessControlCharacteristic, new byte[] { 0x01, 0x06 });

            await Task.Delay(5 * 1000);

            //Interface.Unsubscribe(AccessControlCharacteristic, GlucometerListener);
            //Interface.Unsubscribe(MeasurementCharacteristic, GlucometerListener);

            return true;
        }

        static void GlucometerListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            Console.WriteLine(JsonSerializer.Serialize(GlucometerMeasurement.ParseBytes(data)));
        }

        ///================================================================================================================= Oximeter Section

        static readonly string OximeterWriteCharacteristic = "0000ff01-0000-1000-8000-00805f9b34fb";
        static byte[] AllOximeterData;

        /// <summary>
        /// Set of instructions to get oximeter measurements
        /// </summary>
        /// <example> Command: Oximeter ff:8d:d6:ea:3c:00 </example>
        private static async Task<bool> CommunicateWithOximeter() {
            string MeasurementCharacteristic = "0000ff02-0000-1000-8000-00805f9b34fb";
            AllOximeterData = new byte[0];

            if (!await Interface.Subscribe(MeasurementCharacteristic, OximeterListener)) { return false; }
            if (!await Interface.WriteData(OximeterWriteCharacteristic, new byte[] { 0x99, 0x00, 0x19 })) { return false; }

            await Task.Delay(5 * 1000);

            OximeterMeasurement latest = null;

            // Split all data to chunks of size 24 byte and parse them to valid measurements
            int DataCount = AllOximeterData.Length / 24;
            for (int i = 0; i < DataCount; i++) {
                byte[] data = new byte[24];
                Array.Copy(AllOximeterData, i * 24, data, 0, 24);
                OximeterMeasurement measurement = OximeterMeasurement.ParseBytes(data);

                // Only store the latest measurement by time
                if (latest == null) {
                    latest = measurement;
                } else if (latest.Start.CompareTo(measurement.Start) < 0) {
                    latest = measurement;
                }
            }

            Console.WriteLine(JsonSerializer.Serialize(latest));

            Interface.Unsubscribe(MeasurementCharacteristic, OximeterListener);

            return true;
        }

        /// <summary>
        /// Oximeter specific listener that gets data and stores it, then writes a response on the peripheral
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static async void OximeterListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            AllOximeterData = AllOximeterData.Concat(data).ToArray();

            if (AllOximeterData.Length % 240 == 0) {
                await Interface.WriteData(OximeterWriteCharacteristic, new byte[] { 0x99, 0x01, 0x1a });
            }
        }

        ///================================================================================================================= Blood Pressure Section

        static BloodPressureMeasurement latestBloodPressureMeasurement = null;

        /// <summary>
        /// Set of instructions to get blood pressure measurements
        /// </summary>
        /// <example> Command: BloodPressure fc:d2:b6:56:15:5d </example>
        static async Task<bool> CommunicateWithBloodPressure() {
            string MeasurementCharacteristic = "00002a35-0000-1000-8000-00805f9b34fb";
            latestBloodPressureMeasurement = null;

            if (!await Interface.Subscribe(MeasurementCharacteristic, BloodPressureListener)) { return false; }

            await Task.Delay(5 * 1000);

            //Interface.Unsubscribe(MeasurementCharacteristic, BloodPressureListener);

            Console.WriteLine(JsonSerializer.Serialize(latestBloodPressureMeasurement));

            return true;
        }

        /// <summary>
        /// Blood pressure specific listener that gets data and stores it, then writes a response on the peripheral
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void BloodPressureListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            BloodPressureMeasurement newest = BloodPressureMeasurement.ParseBytes(data);

            // Only store the latest measurement by time
            if (latestBloodPressureMeasurement == null) {
                latestBloodPressureMeasurement = newest;
                return;
            } else if (latestBloodPressureMeasurement.Timestamp.CompareTo(newest.Timestamp) < 0) {
                latestBloodPressureMeasurement = newest;
            }
        }

        ///================================================================================================================= Weight Scale Section

        static TaskCompletionSource<bool> WeightScaleTask;
        static WeightMeasurement latestWeightMeasurement;
        static readonly string WeightScaleMeasurementCharacteristic = "0000ffe1-0000-1000-8000-00805f9b34fb";

        /// <summary>
        /// Set of instructions to get weight scale measurements
        /// </summary>
        /// <example> Command: WeightScale 34:14:b5:a0:1d:03 </example>
        static async Task<bool> CommunicateWithWeightScale() {
            latestWeightMeasurement = null;
            WeightScaleTask = new TaskCompletionSource<bool>();

            if (!await Interface.Subscribe(WeightScaleMeasurementCharacteristic, WeightScaleListener)) { return false; }

            return await WeightScaleTask.Task;
        }

        /// <summary>
        /// Weight scale specific listener that gets data and stores it, then prints the result when measurement is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void WeightScaleListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);

            if (data.Length == 5) {
                latestWeightMeasurement = WeightMeasurement.ParseBytes(data);
                if (Settings.UpdateWeight) {
                    Console.WriteLine("Update: " + JsonSerializer.Serialize(latestWeightMeasurement));
                }
            } else {
                Console.WriteLine(JsonSerializer.Serialize(latestWeightMeasurement));
                Interface.Unsubscribe(WeightScaleMeasurementCharacteristic, WeightScaleListener);
                WeightScaleTask.SetResult(true);
            }
        }

        ///================================================================================================================= Stethoscope Section

        static FileStream outputAudio = null;

        /// <summary>
        /// Set of instructions to get weight scale measurements
        /// </summary>
        /// <example> Command: Stethoscope fb:dc:1e:37:47:bc </example>
        static async Task<bool> CommunicateWithStethoscope() {
            string MeasurementCharacteristic = "00000003-0000-1000-8000-00805f9b34fb";
            string ModeCharacteristic = "00000008-0000-1000-8000-00805f9b34fb";
            mPrevSerial = -2;
            InitAlgo();

            CreateOutputAudioFile();

            //if (!await Interface.Subscribe(ModeCharacteristic, StethoscopeModeListener)) { return false; }
            if (!await Interface.Subscribe(MeasurementCharacteristic, StethoscopeListener)) { return false; }

            await Task.Delay((Settings.AudioLength + 2) * 1000);

            //Interface.Unsubscribe(ModeCharacteristic, StethoscopeModeListener);

            StethoscopeMeasurement result = new StethoscopeMeasurement {
                FileName = outputAudio.Name
            };

            Console.WriteLine(JsonSerializer.Serialize(result));

            Interface.Unsubscribe(MeasurementCharacteristic, StethoscopeListener);

            return true;
        }

        /// <summary>
        /// This function creates the output file to dump the decoded audio data into it.
        /// </summary>
        static void CreateOutputAudioFile() {
            string fileName = $"./{DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss")}.mp3";
            File.Delete(fileName);
            outputAudio = File.Open(fileName, FileMode.OpenOrCreate);
            outputAudio.SetLength(0);

            /// Standard header data
            var data = new byte[] {
                0x52, 0x49, 0x46, 0x46, // RIFF
                0x2C, 0x53, 0x07, 0x00, // file size

                0x57, 0x41, 0x56, 0x45, // WAVE
                0x66, 0x6d, 0x74, 0x20, // fmt
                0x10, 0x00, 0x00, 0x00, // subchunk 1 size

                0x01, 0x00, // audio format
                0x01, 0x00, // channels
                0x40, 0x1F, 0x00, 0x00, // sample rate
                0x80, 0x3E, 0x00, 0x00, // byte rate
                0x02, 0x00, // block align
                0x10, 0x00, // bits per sample

                0x64, 0x61, 0x74, 0x61, // data
                0x00, 0x53, 0x07, 0x00, // subchunk 2 size
            };

            foreach (byte c in data) {
                outputAudio.WriteByte(c);
            }
        }

        /// <summary>
        /// This function initializes the data processors inside .dll files
        /// </summary>
        private static void InitAlgo() {
            SmarthoAlgo.aes_process_init();
            SmarthoAlgo.smarthoAlgo_aec_process_init();
            SmarthoAlgo.smarthoAlgo_init_agcProcess();
            SmarthoAlgo.smarthoAlgo_init_clip_distortion();
            SmarthoAlgo.init_noise_suppression();
            Logger.Log("initAlgo");
        }

        /// <summary>
        /// These data are needed to decode the audio data
        /// </summary>
        private static int mPrevSerial = -2;
        private static byte[] micAudioArr = null;
        private static byte[] spkAudioArr = null;
        private static short[] decodeSpkArr = null;
        private static short[] decodeMicArr = null;
        private static short[] aecOut = null;
        private static short[] gainResultData = null;
        private static EchoMode curEchoMode = EchoMode.MODE_BELL_ECHO;

        /// <summary>
        /// Stethoscope mode listener to update the device mode (BELL or DIAFRAGM) inside the cli
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void StethoscopeModeListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            if (data[0] == 0) {
                curEchoMode = EchoMode.MODE_BELL_ECHO;
            } else if (data[0] == 1) {
                curEchoMode = EchoMode.MODE_DIAPHRAGM_ECHO;
            }
            Logger.Log($"Echo Mode: {curEchoMode}");
        }

        /// <summary>
        /// Stethoscope specific data listener that receives the raw encoded data and decodes it to meaningful audio data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void StethoscopeListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);

            if (data.Length == 158) {
                int num = data[2] & 0xFF;
                if (mPrevSerial != -2 && mPrevSerial + 1 != num) {
                    Logger.Log("data lost");
                }

                if (num == 255) {
                    mPrevSerial = -1;
                } else {
                    mPrevSerial = num;
                }

                spkAudioArr = new byte[30];
                micAudioArr = new byte[120];
                Array.Copy(data, 8, spkAudioArr, 0, spkAudioArr.Length);
                Array.Copy(data, 38, micAudioArr, 0, micAudioArr.Length);
                decodeSpkArr = DecodeAudioDataUtils.GetInstance.DecodeSpkData(spkAudioArr);
                decodeMicArr = DataUtil.byteArray2ShortArray(micAudioArr);

            } else if (data.Length == 150) {
                spkAudioArr = new byte[30];
                micAudioArr = new byte[120];
                Array.Copy(data, 0, spkAudioArr, 0, spkAudioArr.Length);
                Array.Copy(data, 30, micAudioArr, 0, micAudioArr.Length);
                decodeSpkArr = DecodeAudioDataUtils.GetInstance.DecodeSpkData(spkAudioArr);
                decodeMicArr = DataUtil.byteArray2ShortArray2(micAudioArr);

            } else if (data.Length == 180) {
                micAudioArr = new byte[90];
                spkAudioArr = new byte[90];
                Array.Copy(data, 0, micAudioArr, 0, micAudioArr.Length);
                Array.Copy(data, 90, spkAudioArr, 0, spkAudioArr.Length);
                decodeSpkArr = DecodeAudioDataUtils.GetInstance.DecodeSpkData(spkAudioArr);
                decodeMicArr = DecodeAudioDataUtils.GetInstance.DecodeMicData(micAudioArr);
            }

            if (decodeMicArr == null || decodeSpkArr == null) {
                return;
            }

            aecOut = HandleRawData.GetInstance.aecProcessData(decodeMicArr, decodeSpkArr);
            if (aecOut != null) {
                gainResultData = HandleRawData.GetInstance.clipDistortionAndAutoGain(aecOut, curEchoMode);
                if (gainResultData != null) {
                    //handleHeartRate.handleHeartRate(gainResultData);
                    SmarthoAlgo.noise_suppression_process(gainResultData, gainResultData.Length);

                    /// Append the decoded data to the end of the audio file
                    for (int i = 0; i < gainResultData.Length; i++) {
                        outputAudio.WriteByte(DataUtil.Short2Bytes(gainResultData[i])[0]);
                        outputAudio.WriteByte(DataUtil.Short2Bytes(gainResultData[i])[1]);
                    }
                }
            }
        }
    }
}