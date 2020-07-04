using System;
using System.Threading;
using EightAmps;

namespace InfrareddyExample
{
    class Program
    {
        private static string SERRO_5 = "0000 0070 0000 001A 0010 000E 0010 000E 0010 001F 0010 001F 0010 000E 0010 000E 0010 000E 0010 000E 0010 000E 0010 001F 0010 000E 0010 001F 0010 000E 0010 000E 0010 001F 0010 000E 0010 000E 0010 001F 0010 001F 0010 000E 0010 000E 0010 000E 0010 001F 0010 000E 0010 000E 0010 3078";
        private static string SERRO_6 = "0000 0070 0000 001A 0010 000E 0010 000E 0010 001F 0010 001F 0010 000E 0010 000E 0010 000E 0010 000E 0010 000E 0010 000E 0010 001F 0010 001F 0010 000E 0010 000E 0010 001F 0010 000E 0010 000E 0010 000E 0010 000E 0010 000E 0010 000E 0010 000E 0010 001F 0010 000E 0010 000E 0010 367D";

        private static string SHORTY = "0000 006C 0000 0009 00AD 00AD 0016 0041 0016 0041 " +
                                              "0016 0041 0016 0016 0016 0016 0016 0016 0016 0016 0016";

        private static string SAMSUNG_PRONTO_PWR2 = "0000 0073 0000 0022 0000 000E 0004 0004 0004 0001 0005 0001 0005 0001 0006 0000 0006 0000 0007 0000 0007 0000 0008 0000 0008 0001 0009 0001 0009 0001 000A 0000 000A 0000 000B 0000 000B 0000 000C 0000 000C 0000 000D 0001 000E 0000 000E 0000 000F 0000 000F 0000 0010 0000 0010 0000 0011 0001 0011 0000 0012 0001 0012 0001 0013 0001 0013 0001 0014 0001 0014 0001";

        private static string SAMSUNG_PRONTO_PWR = "0000 006C 0000 0022 00AD 00AD 0016 0041 0016 0041 " +
                                              "0016 0041 0016 0016 0016 0016 0016 0016 0016 0016 0016 " +
                                              "0016 0016 0041 0016 0041 0016 0041 0016 0016 0016 0016 " +
                                              "0016 0016 0016 0016 0016 0016 0016 0016 0016 0041 0016 " +
                                              "0016 0016 0016 0016 0016 0016 0016 0016 0016 0016 0016 " +
                                              "0016 0041 0016 0016 0016 0041 0016 0041 0016 0041 0016 " +
                                              "0041 0016 0041 0016 0041 0016 06FB";

        private static string SAMSUNG_PWR3 = "0000 0070 0000 0043 00A6 00A8 0013 003F 0013 003F 0014 003F 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 003F 0013 003F 0013 003F 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 003F 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 003F 0013 0015 0013 003F 0014 003F 0013 003F 0013 003F 0014 003F 0013 003F 0013 06D5 00A5 00A8 0013 003F 0013 003F 0013 003F 0014 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 003F 0014 003F 0013 003F 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 003F 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 0015 0013 003F 0013 0015 0013 003F 0013 003F 0013 003F 0013 003F 0013 003F 0014 003F";

        private static string CMD_CTR_1 = "0000 0070 0000 0023 0149 00A7 0013 0015 0014 0015 0014 0015 0013 0015 0014 0015 0014 0015 0013 0015 0014 0015 0014 003F 0013 0040 0012 0040 0013 003F 0013 0040 0012 0040 0013 003F 0013 0040 0012 0015 0014 0040 0012 0040 0013 003F 0013 0015 0014 0015 0013 0015 0014 0015 0014 003F 0013 0015 0014 0015 0013 0015 0014 0040 0012 0040 0013 003F 0013 0040 0012 05CB 0148 0053";

        static void TestSerroButtons(Infrareddy instance)
        {
            Console.WriteLine("About to encode SERRO 5: {0}", SERRO_5);
            var encodeResult = instance.EncodePronto(SERRO_5, Infrareddy.NoRepeat);
            Console.WriteLine("EncodeResult: {0}", encodeResult);

            Console.WriteLine("About to encode SERRO 6: {0}", SERRO_6);
            encodeResult = instance.EncodePronto(SERRO_6, Infrareddy.NoRepeat);
            Console.WriteLine("EncodeResult: {0}", encodeResult);

            Console.WriteLine("About to encode SERRO 5: {0}", SERRO_5);
            encodeResult = instance.EncodePronto(SERRO_5, Infrareddy.NoRepeat);
            Console.WriteLine("EncodeResult: {0}", encodeResult);

            Console.WriteLine("About to encode SERRO 6: {0}", SERRO_6);
            encodeResult = instance.EncodePronto(SERRO_6, Infrareddy.NoRepeat);
            Console.WriteLine("EncodeResult: {0}", encodeResult);
        }

        static void TestCommandCenterButtons(Infrareddy instance)
        {
            Console.WriteLine("About to encode CMD_CTR_1 {0}", CMD_CTR_1);
            var encodeResult = instance.EncodePronto(CMD_CTR_1, Infrareddy.NoRepeat);
            Console.WriteLine("EncodeResult: {0}", encodeResult);
        }

        static void TestDecodeAndEncodeRandom(Infrareddy instance)
        {
            var startTime = DateTime.Now;
            // Decode Pronto
            var result = instance.DecodePronto();
            Console.WriteLine("DecodeResult.status: {0}", result.status);
            Console.WriteLine("DecodeResult.payload: {0}", result.payload);

            var duration = DateTime.Now - startTime;
            Console.WriteLine("Demo IR Complete in {0}ms", duration.TotalMilliseconds);

            if (result.status == Infrareddy.RequestStatus.IR_SUCCESS)
            {
                Thread.Sleep(500);
                // Encode
                Console.WriteLine("About to encode {0}", result.payload);
                var encodeResult = instance.EncodePronto(result.payload, Infrareddy.NoRepeat);
                Console.WriteLine("EncodeResult: {0}", encodeResult);
            }
            else
            {
                Console.WriteLine("DECODE FAILED: {0}", result.status);
            }
        }

        static void TestDecodeRaw(Infrareddy instance)
        {
            var result = instance.DecodeRaw();
            Console.WriteLine("DecodeResult.status: {0}", result.status);
            Console.WriteLine("DecodeResult.payload: {0}", result.payload);
        }

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

                    // TestCommandCenterButtons(instance);
                    // TestSerroButtons(instance);
                    TestDecodeAndEncodeRandom(instance);

                    Thread.Sleep(100);
                }
                catch (Exception err)
                {
                    // Wait a little while and then try to re-establish connection.
                    // This worked great during development when the firmwware was being
                    // flashed and re-flashed.
                    Thread.Sleep(500);
                    instance = Infrareddy.First();
                }
            }
        }
    }
}
