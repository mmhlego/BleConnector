using BleConnector.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using System.Linq;

namespace BleConnector.Ble {
    static class Manager {
        public static async Task<bool> Communicate(DeviceTypes deviceType) {
            switch (deviceType) {
                case DeviceTypes.Oximeter:
                    return await CommunicateWithOximeter();
                case DeviceTypes.Glucometer:
                    return await CommunicateWithGlucometer();
                case DeviceTypes.Thermometer:
                    return await CommunicateWithThermometer();
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
            await Interface.Subscribe("00002a1c-0000-1000-8000-00805f9b34fb", ThermometerListener);

            await Task.Delay(10 * 1000);

            Console.WriteLine(JsonSerializer.Serialize(latestThermometerMeasuremment));

            Interface.Unsubscribe("00002a1c-0000-1000-8000-00805f9b34fb", ThermometerListener);

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

            await Interface.Subscribe(AccessControlCharacteristic, GlucometerListener);
            await Interface.Subscribe(MeasurementCharacteristic, GlucometerListener);

            await Interface.WriteData(AccessControlCharacteristic, new byte[] { 0x01, 0x06 });

            await Task.Delay(5 * 1000);

            Interface.Unsubscribe(AccessControlCharacteristic, GlucometerListener);
            Interface.Unsubscribe(MeasurementCharacteristic, GlucometerListener);

            return true;
        }

        static void GlucometerListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            Console.WriteLine(JsonSerializer.Serialize(GlucometerMeasurement.ParseBytes(data)));
        }

        ///================================================================================================================= Oximeter Section

        static string OximeterWriteCharacteristic = "0000ff01-0000-1000-8000-00805f9b34fb";
        static byte[] AllOximeterData;

        /// <summary>
        /// Set of instructions to get oximeter measurements
        /// </summary>
        /// <example> Command: Oximeter ff:8d:d6:ea:3c:00 </example>
        private static async Task<bool> CommunicateWithOximeter() {
            string MeasurementCharacteristic = "0000ff02-0000-1000-8000-00805f9b34fb";
            AllOximeterData = new byte[0];

            await Interface.Subscribe(MeasurementCharacteristic, OximeterListener);
            await Interface.WriteData(OximeterWriteCharacteristic, new byte[] { 0x99, 0x00, 0x19 });

            await Task.Delay(5 * 1000);

            Interface.Unsubscribe(MeasurementCharacteristic, OximeterListener);

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

            return true;
        }

        /// <summary>
        ///  Oximeter specific listener that gets data and stores it, then writes a response on the peripheral
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
    }
}
