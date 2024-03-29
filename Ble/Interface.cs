﻿using BleConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Security.Cryptography;

namespace BleConnector.Ble {
    static class Interface {
        public static BluetoothLEDevice ConnectedDevice = null;
        static List<GattDeviceService> Services = new List<GattDeviceService>();
        static List<GattCharacteristic> Characteristics = new List<GattCharacteristic>();

        /// <summary>
        /// Set the target device and discover its characteristics
        /// </summary>
        /// <param name="device"></param>
        public static void SetDevice(BluetoothLEDevice device) {
            Services.Clear();
            Characteristics.Clear();
            ConnectedDevice = null;
            if (device == null) { return; }
            ConnectedDevice = device;
            GetAllCharacteristics();
        }

        /// <summary>
        /// Subscribes to specific characteristic
        /// </summary>
        /// <param name="char_uuid"></param>
        /// <param name="listener"></param>
        /// <returns></returns>
        public static async Task<bool> Subscribe(string char_uuid, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> listener) {
            if (!CheckConnection()) {
                // Device not connected
                Logger.Error(ErrorCodes.DeviceNotConnected);
                return false;
            }

            GattCharacteristic gc = GetCharactristic(char_uuid);
            if (gc == null) {
                return false;
            }

            if (!gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate) && !gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify)) {
                // This characteristic does not support notify/indicate operation.
                Logger.Error(ErrorCodes.SubscribeNotSupported);
                return false;
            }

            GattCommunicationStatus status;
            if (gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate)) {
                status = await gc.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Indicate);
            } else {
                status = await gc.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
            }

            Logger.Log(status.ToString());

            if (status == GattCommunicationStatus.Success) {
                gc.ValueChanged += listener;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unsubscribes from specific characteristic
        /// </summary>
        /// <param name="char_uuid"></param>
        /// <param name="listener"></param>
        /// <returns></returns>
        public static bool Unsubscribe(string char_uuid, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> listener) {
            if (!CheckConnection()) {
                // Device not connected
                Logger.Error(ErrorCodes.DeviceNotConnected);
                return false;
            }

            GattCharacteristic gc = GetCharactristic(char_uuid);
            if (gc == null) {
                return false;
            }

            if (!gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate) && !gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify)) {
                // This characteristic does not support indicate/notify operation
                Logger.Error(ErrorCodes.SubscribeNotSupported);
                return false;
            }

            gc.ValueChanged -= listener;

            Logger.Log("Unsubscribe successful");
            return true;
        }

        /// <summary>
        /// Perform write action on a specific characteristic
        /// </summary>
        /// <param name="commands"></param>
        public static async Task<bool> WriteData(string char_uuid, byte[] write_data) {
            if (!CheckConnection()) {
                // Device not connected
                Logger.Error(ErrorCodes.DeviceNotConnected);
                return false;
            }

            GattCharacteristic gc = GetCharactristic(char_uuid);
            if (gc == null) { return false; }
            if (!gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write)) {
                // This characteristic does not support write operation
                Logger.Error(ErrorCodes.WriteNotSupported);
                return false;
            }

            GattCommunicationStatus result = await gc.WriteValueAsync(write_data.AsBuffer());
            Logger.Log($"Result: {result}");

            if (result == GattCommunicationStatus.Success)
                return true;
            return false;
        }

        /// <summary>
        /// Perform read action on a specific characteristic
        /// </summary>
        /// <param name="commands"></param>
        public async static Task<byte[]> ReadData(string char_uuid) {
            byte[] data;

            if (!CheckConnection()) {
                // Device not connected
                Logger.Error(ErrorCodes.DeviceNotConnected);
                return null;
            }

            GattCharacteristic gc = GetCharactristic(char_uuid);
            if (gc == null) { return null; }
            if (!gc.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read)) {
                // This characteristic does not support read operation;
                Logger.Error(ErrorCodes.ReadNotSupported);
                return null;
            }

            GattReadResult result = await gc.ReadValueAsync();

            if (result.Status == GattCommunicationStatus.Success) {
                CryptographicBuffer.CopyToByteArray(result.Value, out data);
                return data;
            }

            return null;
        }

        /// <summary>
        /// Check if device is still connected
        /// </summary>
        /// <returns></returns>
        static bool CheckConnection() {
            return ConnectedDevice != null && ConnectedDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
        }

        /// <summary>
        /// Iterate through found characteristics and return the one we need
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        static GattCharacteristic GetCharactristic(string uuid) {
            foreach (GattCharacteristic characteristic in Characteristics) {
                if (characteristic.Uuid.ToString() == uuid) {
                    return characteristic;
                }
            }
            // Invalid characteristic uuid (not gonna happen because its all tested)
            Logger.Error(ErrorCodes.InvalidCharacteristic);
            return null;
        }

        /// <summary>
        /// Iterate through all services and discover all characteristics
        /// </summary>
        static async void GetAllCharacteristics() {
            Services.Clear();
            Characteristics.Clear();

            if (ConnectedDevice == null) { return; }

            GattDeviceServicesResult services = await ConnectedDevice.GetGattServicesAsync();

            foreach (var s in services.Services) {
                Services.Add(s);
                GattCharacteristicsResult chars = await s.GetCharacteristicsAsync();

                foreach (var c in chars.Characteristics) {
                    Characteristics.Add(c);
                }
            }
        }
    }
}
