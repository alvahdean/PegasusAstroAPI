using PegasusDriver;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace PegasusTest
{
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    class Program
    {
        static void Main(string[] args)
        {
            var device = new PocketPowerBoxDriver();

            string portName;

            if (args.Count() > 0)
            {
                portName = args[0];
            }
            else
            {
                portName = device.GetPorts().FirstOrDefault();
            }

            if(string.IsNullOrWhiteSpace(portName))
            {
                Console.WriteLine("Not port found or specified");
                return;
            }

            Console.WriteLine($"Connecting to {portName}");
            device.Connect(portName);
            var isOk = device.Ping();

            if(!isOk)
            {
                Console.WriteLine($"Device is not repsonding, disconnecting");
                device.Disconnect();
                return;
            }

            Console.WriteLine($"Device is Ok!");
            
            Console.WriteLine($"Disconnecting...");
            device.Disconnect();

            Console.WriteLine($"Done.");
        }
    }
}
