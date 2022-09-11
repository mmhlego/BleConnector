using System;
using Windows.Devices.Bluetooth;

namespace BleConnector.Models {
    static class Logger {
        public static void Log(string message) {
            if (Settings.Debug) {
                Console.WriteLine(message);
            }
        }

        public static void Error(ErrorCodes error) {
            Console.WriteLine(error.ToString());
        }

        public static void Error(BluetoothError error) {
            Console.WriteLine(error.ToString());
        }
    }

    /// <summary>
    /// List of possible errors inside program
    /// </summary>
    enum ErrorCodes {
        // Bluetooth internal errors
        // https://docs.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetootherror?view=winrt-22621
        ConsentRequired,        // 
        DeviceNotConnected,     // Run again
        DisabledByPolicy,       // 
        DisabledByUser,         // Run as admin?
        NotSupported,           // -
        OtherError,             // 
        RadioNotAvailable,      // Request to turn on bluetooth
        ResourceInUse,          // Try again
        Success,                // Success
        TransportNotSupported,  // 

        DeviceNotFound,         // Please turn on the peripheral
        PairingRequired,        // Open the pair app
        InvalidCommand,
        InvalidMacAddress,
        InvalidDeviceType,

        InvalidCharacteristic,

        WriteNotSupported,
        ReadNotSupported,
        SubscribeNotSupported,

    }
}
