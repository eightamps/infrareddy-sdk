using HidSharp.Reports;
using HidSharp;
using LibUsbDotNet;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System;
using System.Security.Cryptography;

namespace EightAmps
{
    public class Infrareddy
    {
        public const UInt16 IR_ENVELOPE_SIZE = (512 - 1);
        public const UInt16 IR_ENCODE_DATA_SIZE = (IR_ENVELOPE_SIZE - (1 + 1 + 2));
        public const UInt16 IR_DECODE_DATA_SIZE = (IR_ENVELOPE_SIZE - (1 + 1 + 4 + 1 + 2));

        public const Byte TYPE_PRONTO = 0x00;
        public const Byte STATUS_REPORT_ID = 0x01;
        public const Byte DEC_CMD_ID = 0x02;
        public const Byte ENC_CMD_ID = 0x03;

        public const uint ASPEN_PRODUCT_ID = 0xff8a0002;

        private static Byte RequestTag = 100;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = IR_ENVELOPE_SIZE)]
        public struct EncodeCmdReportType
        {
            public Byte id;
            public Byte tag;
            public UInt16 len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IR_ENCODE_DATA_SIZE)]
            public Byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 3)]
        public struct StatusRspReportType
        {
            public Byte id;
            public Byte tag;
            public Int32 status;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 5)]
        public struct DecodeCmdReportType
        {
            public Byte id;
            public Byte tag;
            public UInt16 timeoutMs;
            public Byte type;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = IR_ENVELOPE_SIZE)]
        public struct DecodeCmdResponseType
        {
            public Byte id;
            public Byte tag;
            public Int32 status;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IR_DECODE_DATA_SIZE)]
            public Byte type;
            public UInt16 len;
            public Byte[] data;
        }

        public enum RequestStatus
        {
            IR_SUCCESS = 0,
            IR_IS_BUSY,
            IR_INVALID_NOT_HEX = 50,
            IR_INVALID_MALFORMED,
            IR_UNSUPPORTED_FORMAT,
            IR_UNSUPPORTED_FREQUENCY,
            IR_INVALID_SIZE,
            IR_INVALID,
        }

        public struct DecodeProntoResponse
        {
            public RequestStatus status;
            public string payload;
        }

        public static Byte Repeat = 0x01;
        public static Byte NoRepeat = 0x00;
        private HidDevice hiddev { get { return stream.Device; } }
        private HidStream stream;
        public Version SoftwareVersion { get; private set; }
        public Version HardwareVersion { get { return hiddev.ReleaseNumber; } }
        public delegate void CompleteHandler(RequestStatus status);

        public Infrareddy(HidStream hidStream)
        {
            this.stream = hidStream;
        }

        Byte[] StructureToByteArray(object obj)
        {
            var len = Marshal.SizeOf(obj);
            var arr = new Byte[len];
            var ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        void ByteArrayToStructure(Byte[] bytearray, ref object obj)
        {
            var len = Marshal.SizeOf(obj);
            var copy = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, 0, copy, len);
            obj = Marshal.PtrToStructure(copy, obj.GetType());
            Marshal.FreeHGlobal(copy);
        }

        /**
         * Used by tests only to reset the global counter.
         */
        public static void ResetRequestTag()
        {
            RequestTag = 0;
        }

        /**
         * Get the next valid request key.
         * 
         * Valid values are from 0 to 255.
         */
        public static Byte NextRequestTag()
        {
            return RequestTag++;
        }

        private Byte[] ProntoStringToBytes(string command)
        {
            return Encoding.ASCII.GetBytes(command);
        }

        // Convert the C# string command to Byte array.
        public Byte[] ProntoBytesToWireBytes(Byte[] prontoBytes)
        {
            var bytes = new Byte[IR_ENCODE_DATA_SIZE];
            // Copy the values into a longer array so that we can marshal 
            // the Byte array of fixed length into the firmware.
            for (var i = 0;  i < prontoBytes.Length; i++)
            {
                bytes[i] = prontoBytes[i];
            }
            return bytes;
        }

        /**
         * Get the bytes from the provided Report and return
         * them as a string that is limited by chunkLen.
         */
        private string ReportToString(EncodeCmdReportType report)
        {
            return Encoding.ASCII.GetString(report.data)
                .Substring(0, report.len);
        }

        public EncodeCmdReportType ProntoToReport(string pronto, Byte requestTag)
        {
            var prontoBytes = ProntoStringToBytes(pronto);
            var wireBytes = ProntoBytesToWireBytes(prontoBytes);

            return new EncodeCmdReportType
            {
                id = ENC_CMD_ID,
                tag = requestTag,
                len = (UInt16)prontoBytes.Length,
                data = wireBytes,
            };
        }
        /**
         * Tell the hardware to emit the provided Pronto command using either
         * the "once" block if isRepeat is false, or the "repeat" block if
         * isRepeat is true.
         *
         * Call the provided handler with status updates when emit is complete.
         */
        public RequestStatus EncodePronto(string prontoStr, Byte isRepeat)
        {
            Console.WriteLine("EMIT PRONTO Called with: {0}", prontoStr);
            if (prontoStr.Length > IR_ENCODE_DATA_SIZE)
            {
                prontoStr = prontoStr.Substring(0, IR_ENCODE_DATA_SIZE - 4);
                // throw new InvalidOperationException("EmitPronto called with IR code that is too long.");
            }

            var requestTag = Infrareddy.NextRequestTag();
            var report = ProntoToReport(prontoStr, requestTag);
            var writeBytes = StructureToByteArray(report);
            stream.Write(writeBytes);

            var responseBytes = stream.Read();
            object responseObj = new StatusRspReportType { };
            ByteArrayToStructure(responseBytes, ref responseObj);
            StatusRspReportType response = (StatusRspReportType)responseObj;
            return (RequestStatus)response.status;
        }

        /**
         * Tell the hardware to begin listening for IR signals and to return
         * them in Pronto format.
         *
         * Call the provided handler when listening has either received a
         * signal, failed or timed out.
         */
        public DecodeProntoResponse DecodePronto()
        {
            Infrareddy.NextRequestTag();
            var requestTag = Infrareddy.NextRequestTag();
            var command = new DecodeCmdReportType
            {
                id = DEC_CMD_ID,
                tag = requestTag,
            };
            Console.WriteLine("Sending to device");
            stream.Write(StructureToByteArray(command));
            stream.Flush();

            Console.WriteLine("Reading from device");
            var responseBytes = new Byte[IR_ENVELOPE_SIZE];
            stream.Read(responseBytes);
            Console.WriteLine("Read complete!");

            object responseObj = new DecodeCmdResponseType { };
            ByteArrayToStructure(responseBytes, ref responseObj);
            var responseStruct = (DecodeCmdResponseType)responseObj;

            return new DecodeProntoResponse
            {
                status = (RequestStatus)responseStruct.status,
                payload = bytesToString(responseStruct.data),
            };
        }

        private string bytesToString(Byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        /**
         * Return true if the provided HID Device looks like Infrareddy hardware.
         * Specifically, it should have the correct Vendor ID, Product ID and declare the expected
         * USB application usage.
         */
        private static bool IsInfrareddy(HidDevice hiddev)
        {
            return Infrareddy.IsAspenDevice(hiddev) && Infrareddy.GetApplicationUsage(hiddev) == (uint)ASPEN_PRODUCT_ID;
        }

        /**
         * Create a Stream from the provided device.
         */
        private static HidStream StreamFromDevice(HidDevice hidDevice)
        {
            HidStream hidStream = null;
            if (!hidDevice.TryOpen(out hidStream))
            {
                return null;
            }

            hidStream.ReadTimeout = Timeout.Infinite;
            return hidStream;
        }

        /**
         * Get a collection of Infrareddy devices from the provided list of
         * HID devices.
         */
        public static IEnumerable<Infrareddy> AllFrom(IEnumerable<HidDevice> hidDevices)
        {
            var devices = hidDevices.Where(d => Infrareddy.IsInfrareddy(d));

            var instances = new List<Infrareddy>();
            foreach (var device in devices)
            {
                instances.Add(CreateFromHidDevice(device));
            }

            return instances;
        }

        /**
         * Create an Infrareddy instance with a connected stream using the provided device.
         */
        private static Infrareddy CreateFromHidDevice(HidDevice device)
        {
            var stream = StreamFromDevice(device);
            if (stream == null)
            {
                throw new Exception("Expected StreamFromDevice");
            }

            return new Infrareddy(stream);
        }

        /**
         * Get a collection of connected Infrareddy devices by going directly
         * to the hardware listing.
         *
         * Returns an empty collection if no devices are found.
         */
        public static IEnumerable<Infrareddy> All()
        {
            return Infrareddy.AllFrom(DeviceList.Local.GetHidDevices());
        }

        /**
         * Get the first Infrareddy HID device found and return it.
         * Returns null if no device is found and throws if a device is found,
         * but cannot communicate.
         */
        public static Infrareddy First()
        {
            var infrareddies = DeviceList.Local.GetHidDevices().Where(d => Infrareddy.IsInfrareddy(d));
            if (infrareddies.Count() > 0)
            {
                return CreateFromHidDevice(infrareddies.First());
            }
            return null;
        }

        public static bool IsAspenDevice(HidDevice hiddev)
        {
            if (hiddev == null)
                return false;
            if (hiddev.VendorID != 0x0483)
                return false;
            if (hiddev.ProductID != 0xa367)
                return false;
            return true;
        }

        public static bool IsAspenDevice(UsbDevice usbdev)
        {
            if (usbdev == null)
                return false;
            if ((ushort)usbdev.Info.Descriptor.VendorID != 0x0483)
                return false;
            if ((ushort)usbdev.Info.Descriptor.ProductID != 0xa367)
                return false;
            return true;
        }

        public static uint GetApplicationUsage(HidDevice hiddev)
        {
            if (hiddev == null)
            {
                return 0;
            }
            var reportDescriptor = hiddev.GetReportDescriptor();
            var ditem = reportDescriptor.DeviceItems.FirstOrDefault();
            return ditem.Usages.GetAllValues().FirstOrDefault();
        }

        public static Byte[] CreateBuffer(Report report)
        {
            var buffer = new Byte[report.Length];
            buffer[0] = report.ReportID;
            return buffer;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}