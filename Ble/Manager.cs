using BleConnector.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Security.Cryptography;

namespace BleConnector.Ble {
    static class Manager {
        public static async Task<bool> Communicate(DeviceTypes deviceType) {
            switch (deviceType) {
                //case DeviceTypes.Oximeter:
                //return CommunicateWithOximeter();
                //case DeviceTypes.Glucometer:
                //return CommunicateWithGlucometer();
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

            return true;
        }
        static void ThermometerListener(GattCharacteristic sender, GattValueChangedEventArgs args) {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
            latestThermometerMeasuremment = ThermometerMeasurement.ParseBytes(data);
        }

        ///================================================================================================================= Glucometer Section

        private static bool CommunicateWithGlucometer() {
            throw new NotImplementedException();
        }

        ///================================================================================================================= Oximeter Section

        private static bool CommunicateWithOximeter() {
            throw new NotImplementedException();
        }
    }
}
