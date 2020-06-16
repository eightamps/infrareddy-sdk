using System;
using System.Linq;
using EightAmps;
using HidSharp;

namespace SwitchyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Switchy switchy = null;
            if (!DeviceList.Local.GetHidDevices().Any(d => Switchy.TryOpen(d, out switchy)))
            {
                Console.WriteLine("No Switchy found, exiting.");
            }
            else
            {
                using (switchy)
                {
                    Console.WriteLine("Aspen version: " + switchy.HardwareVersion);
                    Console.WriteLine("Switchy version: " + switchy.SoftwareVersion);
                    Console.WriteLine("Currently loaded configuration: " + switchy.Config);
                    Console.WriteLine("Setting Volume up and down keys:");
                    switchy.Config = new Switchy.ConsumerConf()
                        .SetKey(0, Switchy.ConsumerConf.Key.KVolumeUp)
                        .SetKey(1, Switchy.ConsumerConf.Key.KVolumeDown);
                }
            }
        }
    }
}
