using System;
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
            // Get the first connected Infrareddy device.
            var instance = Infrareddy.First();
            // Bail if we don't find an Infrareddy device.
            if (instance == null)
            {
                Console.WriteLine("No Infrareddy found, reconnect the USB device and try again.");
                return;
            }

            Console.WriteLine("HardwareVersion: {0}", instance.HardwareVersion);

            // Get the first connected Infrareddy device from the collection.
            while (true)
            {
                var startTime = DateTime.Now;
                var result = instance.EmitPronto(SAMSUNG_PRONTO_PWR, Infrareddy.Repeat);
                var duration = DateTime.Now - startTime;
                Console.WriteLine("EMIT IR Complete in {0}ms", duration.TotalMilliseconds);
                if (result != Infrareddy.RequestStatus.IR_SUCCESS)
                {
                    Console.WriteLine("[ERROR] Code: {0}, Press any key to continue.", result);
                    Console.ReadLine();
                }

            }

            // Also try listening...
            // instance.ListenPronto((payload) =>
            // {
                // Console.WriteLine("ListenPronto Complete with status: {0} value: {1} isRepeat: {2}", payload.Status, payload.Value2, payload.IsRepeat);
            // });
        }
    }
}
