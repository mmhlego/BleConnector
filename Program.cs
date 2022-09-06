using BleConnector.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BleConnector.Ble;
using System.Text.Json;
using System.Threading;

/*
   *      Input format:
   *          <device type> <mac address>
   *      
   *      First we scan the environment for current device. If found, we'll connect and get its measurements according to <device type>
   */

//if (commands[0].Equals('q')) {
//    return;
//}

string[] commands = Environment.GetCommandLineArgs().Skip(1).ToArray();

// Check if the command is valid
if (commands.Length != 2) {
    // Invalid command format. Format: <Device_Type> <Mac_Address>
    Logger.Error(ErrorCodes.InvalidCommand);
    return;
}

// Check if device type is valid
if (!Enum.TryParse(commands[0], out DeviceTypes deviceType)) {
    //Invalid device type. Valid device types: {Enum.GetValues(typeof(DeviceTypes)}
    //Logger.Error(ErrorCodes.InvalidDeviceType);
    Console.WriteLine(ErrorCodes.InvalidCommand);
    return;
}

// Check if mac address is valid
if (!Regex.IsMatch(commands[1], "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})")) {
    // Invalid mac address format. Correct format: XX:XX:XX:XX:XX:XX"
    Logger.Error(ErrorCodes.InvalidMacAddress);
    return;
}

//await GenerateRandomData(deviceType);
//return;

// Scan and connect to device
Core.Scan(commands[1]);

if (Interface.ConnectedDevice == null) {
    Logger.Error(ErrorCodes.DeviceNotFound);
    return;
}

// Check if connected (except weight scale)
if (Interface.ConnectedDevice.DeviceInformation.Pairing.CanPair && !Interface.ConnectedDevice.DeviceInformation.Pairing.IsPaired
    && deviceType != DeviceTypes.WeightScale) {
    Logger.Error(ErrorCodes.PairingRequired);
    return;
}

Console.WriteLine("Connected");

// Connect according to the given device type
await Manager.Communicate(deviceType);

async Task<bool> GenerateRandomData(DeviceTypes deviceType) {
    Random random = new Random();

    if (random.Next(5) == 0) {
        Logger.Error(ErrorCodes.DeviceNotFound);
        return false;
    }

    Console.WriteLine("Connected");

    await Task.Delay(2000);

    if (deviceType == DeviceTypes.BloodPressure) {
        Console.WriteLine(JsonSerializer.Serialize(BloodPressureMeasurement.ParseBytes(new byte[] { 0x1E, 0x70, 0x00, 0x4D, 0x00, 0x00, 0x00, 0xDF, 0x07, 0x01, 0x0E, 0x0A, 0x37, 0x00, 0x48, 0x00, 0x01, 0x00, 0x00 })));

    } else if (deviceType == DeviceTypes.WeightScale) {
        //for (int i = 0; i < 5; i++) {
        //    Console.WriteLine("Update: " + JsonSerializer.Serialize(new WeightMeasurement { Weight = 90 + random.NextDouble() * 5, }));
        //    await Task.Delay(1000);
        //}
        Console.WriteLine(JsonSerializer.Serialize(new WeightMeasurement { Weight = 90 + random.NextDouble() * 5, }));

    } else if (deviceType == DeviceTypes.Oximeter) {
        Console.WriteLine(JsonSerializer.Serialize(OximeterMeasurement.ParseBytes(new byte[] { 0xE9, 0x00, 0x0F, 0x0B, 0x06, 0x0D, 0x14, 0x25, 0x0F, 0x0B, 0x06, 0x0D, 0x14, 0x3A, 0x00, 0x00, 0x16, 0x60, 0x60, 0x60, 0x6F, 0x6E, 0x6E, 0x4B })));

    } else if (deviceType == DeviceTypes.Glucometer) {
        Console.WriteLine(JsonSerializer.Serialize(GlucometerMeasurement.ParseBytes(new byte[] { 22, 9, 0, 230, 7, 8, 5, 10, 10, 0, 55, 192, 17 })));

    } else if (deviceType == DeviceTypes.Thermometer) {
        Console.WriteLine(JsonSerializer.Serialize(ThermometerMeasurement.ParseBytes(new byte[] { 7, 226, 3, 0, 255, 225, 7, 4, 13, 14, 7, 0, 255 })));
    }

    return true;
}