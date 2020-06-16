using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EightAmps;

namespace InfrareddyExample
{
    class Program
    {
        private static string PANA_PRONTO = "0000 0070 0000 0032 0080 0040 0010 0010 0010 0030 0010 " +
                                  "0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 " +
                                  "0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 " +
                                  "0030 0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 " +
                                  "0010 0010 0010 0010 0010 0030 0010 0010 0010 0030 0010 " +
                                  "0010 0010 0010 0010 0030 0010 0030 0010 0030 0010 0010 " +
                                  "0010 0010 0010 0010 0010 0030 0010 0010 0010 0030 0010 " +
                                  "0030 0010 0030 0010 0030 0010 0010 0010 0010 0010 0030 " +
                                  "0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 0010 " +
                                  "0010 0010 0030 0010 0ACD";

        private static string RC5_PRONTO = "0000 0073 0000 000C 0020 0020 0040 0020 0020 0020 0020 " +
                                  "0020 0020 0020 0020 0020 0020 0020 0020 0040 0040 0020 " + 
                                  "0020 0020 0020 0020 0020 0CC8";

        private static string DSN_FAN_PWR = "0000 006d 0022 0000 0054 001f 001c 003c 001c 001f 001c " +
                                  "003c 001c 001f 001c 001f 001c 001f 001c 001f 001c 0021 " +
                                  "001c 0020 001c 0020 001c 0020 001c 0020 001c 0020 001c " +
                                  "003d 001c 003d 001c 0f7f 0054 001f 001c 003c 001c 001f " +
                                  "001c 003c 001c 001f 001c 001f 001c 001f 001c 001f 001c " +
                                  "0021 001c 0020 001c 0020 001c 0020 001c 0020 001c 0020 " +
                                  "001c 003d 001c 003d 001c 001f";

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
                instance.EmitPronto(DSN_FAN_PWR, Infrareddy.NoRepeat, (Infrareddy.RequestStatus status) =>
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
