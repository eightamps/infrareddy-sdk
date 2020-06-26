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
            int count = 0;

            // Get the first connected Infrareddy device from the collection.
            while (true)
            {
                try
                {
                    count++;
                    Console.WriteLine("Count: {0}", count);

                    var startTime = DateTime.Now;
                    // Encode
                    // Console.WriteLine("About to encode {0}", SAMSUNG_PRONTO_PWR);
                    // var encodeResult = instance.EncodePronto(SAMSUNG_PRONTO_PWR, Infrareddy.NoRepeat);
                    // Console.WriteLine("EncodeResult: {0}", encodeResult);

                    // Decode
                    var result = instance.DecodePronto();
                    Console.WriteLine("DecodeResult.status: {0}", result.status);
                    Console.WriteLine("DecodeResult.payload: {0}", result.payload);

                    var duration = DateTime.Now - startTime;
                    Console.WriteLine("Demo IR Complete in {0}ms", duration.TotalMilliseconds);

                    if (result.status == Infrareddy.RequestStatus.IR_SUCCESS)
                    {
                        Thread.Sleep(500);
                        // Encode
                        // Console.WriteLine("About to encode {0}", result.payload);
                        var encodeResult = instance.EncodePronto(result.payload, Infrareddy.NoRepeat);
                        Console.WriteLine("EncodeResult: {0}", encodeResult);
                    }
                    else
                    {
                        Console.WriteLine("DECODE FAILED: {0}", result.status);
                    }

                    // Thread.Sleep(100);
                }
                catch (Exception err)
                {
                    // Wait a little while and then try to re-establish connection.
                    Thread.Sleep(500);
                    instance = Infrareddy.First();
                }
            }
        }
    }
}
