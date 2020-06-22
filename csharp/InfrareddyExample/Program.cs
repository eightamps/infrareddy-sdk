using System;
using System.Threading;
using EightAmps;

namespace InfrareddyExample
{
    class Program
    {
        private static string SHORTY = "0000 006C 0000 0009 00AD 00AD 0016 0041 0016 0041 " +
                                              "0016 0041 0016 0016 0016 0016 0016 0016 0016 0016 0016";

        private static string SAMSUNG_PRONTO_PWR = "0000 006C 0000 0022 00AD 00AD 0016 0041 0016 0041 " +
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
                // Encode
                var encodeResult = instance.EncodePronto(SAMSUNG_PRONTO_PWR, Infrareddy.NoRepeat);
                Console.WriteLine("EncodeResult: {0}", encodeResult);

                var result = instance.DecodePronto();
                var duration = DateTime.Now - startTime;
                Console.WriteLine("EMIT IR Complete in {0}ms", duration.TotalMilliseconds);
                if (result.status != Infrareddy.RequestStatus.IR_SUCCESS)
                {
                    Console.WriteLine("[ERROR] Code: {0}, Press any key to continue.", result);
                    Console.ReadLine();
                }

                Console.WriteLine("DATA: {0}", result.payload);

                Thread.Sleep(500);
            }

            // Also try listening...
            // instance.ListenPronto((payload) =>
            // {
                // Console.WriteLine("ListenPronto Complete with status: {0} value: {1} isRepeat: {2}", payload.Status, payload.Value2, payload.IsRepeat);
            // });
        }
    }
}
