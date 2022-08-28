using BleConnector.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BleConnector.Ble;

/// <summary>
/// Program entry point
/// </summary>
namespace BleConnector {
    internal class Program {
        static async Task Main(string[] args) {
            while (true) {
                Console.Write("> ");
                /*
                   *      Input format:
                   *          <device type> <mac address>
                   *      
                   *      First we scan the environment for current device. If found, we'll connect and get its measurements according to <device type>
                   */
                string[] commands = Console.ReadLine().Split(' ');

                if (commands[0].Equals('q')) {
                    return;
                }

                // Check if the command is valid
                if (commands.Length != 2) {
                    Console.WriteLine("Invalid command format. Format: <Device_Type> <Mac_Address>");
                    continue;
                }

                // Check if device type is valid
                if (!Enum.TryParse(commands[0], out DeviceTypes deviceType)) {
                    Console.WriteLine($"Invalid device type. Valid device types: {Enum.GetValues(typeof(DeviceTypes))}");
                    continue;
                }

                // Check if mac address is valid
                if (!Regex.IsMatch(commands[1], "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})")) {
                    Console.WriteLine("Invalid mac address format. Correct format: XX:XX:XX:XX:XX:XX");
                    continue;
                }

                // Scan and connect to device
                Core.Scan(commands[1]);

                if (Interface.ConnectedDevice == null) {
                    Console.WriteLine("Device not found.");
                    continue;
                }

                // Check if connected
                if (Interface.ConnectedDevice.DeviceInformation.Pairing.CanPair && !Interface.ConnectedDevice.DeviceInformation.Pairing.IsPaired) {
                    Console.WriteLine("Pairing required.");
                    continue;
                }

                // Connect according to the given device type
                await Manager.Communicate(deviceType);
            }
        }
    }
}
