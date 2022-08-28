
using BleConnector.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace BleConnector.Ble {
    static class Core {
        static BluetoothLEAdvertisementWatcher BleWatcher = new BluetoothLEAdvertisementWatcher {
            ScanningMode = BluetoothLEScanningMode.Active
        };

        static string TargetMacAddress = "";

        // Scans the environment for device with given mac address
        public static void Scan(string macAddress) {
            TargetMacAddress = macAddress.ToLower();

            StartWatcher();

            // Stop watcher after 10 seconds
            Thread.Sleep(10000);
            BleWatcher.Stopped -= OnScanError;
            BleWatcher.Stop();
            BleWatcher.Received -= OnScanResults;
        }

        // Simplifies the watcher start process
        static void StartWatcher() {
            BleWatcher.Stopped += OnScanError;
            BleWatcher.Received += OnScanResults;
            BleWatcher.Start();
        }

        // Gets called when a new device gets found
        static async void OnScanResults(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv) {
            BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);

            // return if device with address of btAdv.BluetoothAddress couldn't be found
            if (device == null) { return; }

            // convert device address(long) to mac address format
            var macAddress = Regex.Replace(device.BluetoothAddress.ToString("X"), "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})", "$1:$2:$3:$4:$5:$6");

            Logger.Log($"Found {macAddress}");

            // check if target device is found
            if (!macAddress.ToLower().Equals(TargetMacAddress)) { return; }

            // Stop the watcher and save the target device
            BleWatcher.Stopped -= OnScanError;
            BleWatcher.Stop();
            Interface.SetDevice(device);
        }

        // Gets called when an error occurs in scan process
        static void OnScanError(BluetoothLEAdvertisementWatcher s, BluetoothLEAdvertisementWatcherStoppedEventArgs e) {
            BleWatcher.Stop();
            Logger.Error(e.Error);
        }
    }
}
