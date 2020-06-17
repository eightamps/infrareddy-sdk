using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EightAmps;

namespace InfrareddyExample
{
    class Program
    {
        private static string SAMSUNG_PRONTO_PWR = "0000 006C 0000 0022 00AD 00AD 0016 0041 0016 0041" +
                                  "0016 0041 0016 0016 0016 0016 0016 0016 0016 0016 0016 " +
                                  "0016 0016 0041 0016 0041 0016 0041 0016 0016 0016 0016 " +
                                  "0016 0016 0016 0016 0016 0016 0016 0016 0016 0041 0016 " +
                                  "0016 0016 0016 0016 0016 0016 0016 0016 0016 0016 0016 " +
                                  "0016 0041 0016 0016 0016 0041 0016 0041 0016 0041 0016 " +
                                  "0041 0016 0041 0016 0041 0016 06FB";

        static void Main(string[] args)
        {
            // Get all connected Infrareddy devices.
            IEnumerable<Infrareddy> connectedDevices = Infrareddy.All();
            // Bail if we don't find any Infrareddy devices.
            if (connectedDevices.Count() == 0)
            {
                Console.WriteLine("No Infrareddy found, reconnect the USB device and try again.");
                return;
            }
            // Get the first connected Infrareddy device from the collection.
            Infrareddy instance = connectedDevices.FirstOrDefault();
            while (true)
            {
                var startTime = DateTime.Now;
                instance.EmitPronto(SAMSUNG_PRONTO_PWR, Infrareddy.Repeat, (Infrareddy.RequestStatus status) =>
                {
                    var duration = DateTime.Now - startTime;
                    Console.WriteLine("EMIT IR Complete in {0}ms", duration.TotalMilliseconds);
                });

                Thread.Sleep(1000);
            }

            // Also try listening...
            // instance.ListenPronto((payload) =>
            // {
                // Console.WriteLine("ListenPronto Complete with status: {0} value: {1} isRepeat: {2}", payload.Status, payload.Value2, payload.IsRepeat);
            // });
        }
    }
}
