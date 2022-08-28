﻿using System;
using Windows.Devices.Bluetooth;

namespace BleConnector.Models {
    static class Logger {
        static bool IsDev = true;

        public static void Log(string message) {
            if (IsDev) {
                Console.WriteLine(message);
            }
        }

        public static void Error(ErrorCodes error) {
            Console.Write(error.ToString());
        }

        public static void Error(BluetoothError error) {
            Console.Write(error.ToString());
        }
    }
    enum ErrorCodes {
        // Bluetooth internal errors
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
        ScanFailed,             //
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
