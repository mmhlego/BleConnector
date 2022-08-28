using BleConnector.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Program entry point
/// </summary>
namespace BleConnector {
    internal class Program {
        static void Main(string[] args) {
            Task.Run(async () => {
                while (true) {
                    /*
                       *      Input format:
                       *          <device type> <mac address>
                       *      
                       *      First we scan the environment for current device. If found, we'll connect and get its measurements according to <device type>
                       */
                    string[] commands = Console.ReadLine().Split(' ');
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

                    // TODO: Scan and connect to device

                    // TODO: Check if connected

                    // TODO: Get bytes and parse to measurements

                    // TODO: Print data in json format
                }
            });
        }
    }
}
