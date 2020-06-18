using HidSharp.Reports;
using HidSharp;
using LibUsbDotNet;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System;

namespace EightAmps
{
    public class Infrareddy
    {
        public const UInt16 IR_DATA_PACKET_SIZE = (64 - (1 + 2 + 2 + 2 + 2) - 1);
        public const UInt16 IR_DATA_MESSAGE_SIZE = (4096 - 1);
        public const Byte STATUS_REPORT_ID = 0x01;
        public const Byte TYPE_PRONTO = 0x00;
        public const Byte ENC_CMD_ID = 0x03;
        public const uint PRODUCT_ID = 0xff8a0002;

        private static UInt16 RequestTag = 0;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct IrDataProt
        {
            public UInt16 chunkOffset;
            public UInt16 chunkLen;
            public UInt16 len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IR_DATA_PACKET_SIZE)]
            public Byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct EncodeCmdReportType
        {
            public Byte id;
            public UInt16 tag;
            [MarshalAs(UnmanagedType.Struct)]
            public IrDataProt prot;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct StatusRspReportType
        {
            public Byte id;
            public UInt16 tag;
            public Byte status;
        }

        public enum RequestStatus
        {
            IR_SUCCESS = 0,
            IR_INVALID_NOT_HEX,
            IR_INVALID_MALFORMED,
            IR_UNSUPPORTED_FORMAT,
            IR_UNSUPPORTED_FREQUENCY,
            IR_INVALID_SIZE,
            IR_IS_BUSY,
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
            var i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, 0, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);
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
         * Valid values are from 0 to 254.
         */
        public static UInt16 NextRequestTag()
        {
            // Wrap back to 0 at UInt16 max value
            if (RequestTag == 0xffff)
            {
                RequestTag = 0;
            }

            return RequestTag++;
        }

        // Convert the C# string command to Byte array.
        public Byte[] CommandToBytes(string command)
        {
            return Encoding.ASCII.GetBytes(command);
        }

        /**
         * Get the bytes from the provided Report and return
         * them as a string that is limited by chunkLen.
         */
        private string ReportToString(EncodeCmdReportType report)
        {
            return Encoding.ASCII.GetString(report.prot.data)
                .Substring(0, report.prot.chunkLen);
        }

        /**
         * Generate an encoded IR Pronto Command string by combining
         * the provided report data.
         */
        public string ReportsToCommand(EncodeCmdReportType[] reports)
        {
            var result = "";
            for (var i = 0; i < reports.Length; i++)
            {
                result += ReportToString(reports[i]);
            }
            return result;
        }

        /**
         * Convert an IR command string into a set of data packets for transmission
         * over the 64-Byte USB HID channel.
         */
        public EncodeCmdReportType[] CommandToReports(string command, UInt16 requestTag)
        {
            var commandBytes = CommandToBytes(command);
            var packetCount = (commandBytes.Length + IR_DATA_PACKET_SIZE - 1) / IR_DATA_PACKET_SIZE;
            var packets = new EncodeCmdReportType[packetCount];

            for (var i = 0; i < packetCount; i++)
            {
                var totalLen = (UInt16)commandBytes.Length;
                var chunkOffset = (UInt16)(i * IR_DATA_PACKET_SIZE);
                var chunkLen = (UInt16)Math.Min(IR_DATA_PACKET_SIZE, totalLen - chunkOffset);
                var packet = new EncodeCmdReportType
                {
                    id = ENC_CMD_ID,
                    tag = requestTag,
                    prot = new IrDataProt
                    {
                        chunkOffset = chunkOffset,
                        chunkLen = chunkLen,
                        len = totalLen,
                        data = new Byte[IR_DATA_PACKET_SIZE],
                    }
                };

                // Copy the data into the destination array.
                Array.Copy(commandBytes, chunkOffset, packet.prot.data, 0, chunkLen);
                packets[i] = packet;
            }

            return packets;
        }

        public EncodeCmdReportType CommandToReport(string command, UInt16 requestTag)
        {
            var bytes = CommandToBytes(command);

            return new EncodeCmdReportType {
                id = ENC_CMD_ID,
                tag = requestTag,
                prot = new IrDataProt
                {
                    chunkOffset = 0,
                    chunkLen = IR_DATA_PACKET_SIZE,
                    len = (UInt16)bytes.Length,
                    data = bytes,
                }
            };
        }
        /**
         * Tell the hardware to emit the provided Pronto command using either
         * the "once" block if isRepeat is false, or the "repeat" block if
         * isRepeat is true.
         *
         * Call the provided handler with status updates when emit is complete.
         */
        public RequestStatus EmitPronto(string command, Byte isRepeat)
        {
            Console.WriteLine("EMIT PRONTO Called with: {0}", command);
            if (command.Length > IR_DATA_MESSAGE_SIZE)
            {
                throw new InvalidOperationException("EmitPronto called with IR code that is too long.");
            }

            var requestTag = Infrareddy.NextRequestTag();
            var reports = CommandToReports(command, requestTag);
            for (var i = 0; i < reports.Length; i++)
            {
                stream.Write(StructureToByteArray(reports[i]));
            }

            var bytes = stream.Read();
            string hex = BitConverter.ToString(bytes);
            hex.Replace("-", "");
            Console.WriteLine("BYTES: {0}", hex);

            object responseObj = new StatusRspReportType { };
            ByteArrayToStructure(bytes, ref responseObj);
            StatusRspReportType response = (StatusRspReportType)responseObj;

            Console.WriteLine("Response.status {0}", response.status);

            Thread.Sleep(500);

            return (RequestStatus)response.status;
        }

        /**
         * Tell the hardware to begin listening for IR signals and to return
         * them in Pronto format.
         *
         * Call the provided handler when listening has either received a
         * signal, failed or timed out.
         */
        public void ListenPronto(CompleteHandler handler)
        {
            throw new NotImplementedException("ListenPronto not yet implemented");
        }

        /**
         * Return true if the provided HID Device looks like Infrareddy hardware.
         * Specifically, it should have the correct Vendor ID, Product ID and declare the expected
         * USB application usage.
         */
        private static bool IsInfrareddy(HidDevice hiddev)
        {
            return Infrareddy.IsAspenDevice(hiddev) && Infrareddy.GetApplicationUsage(hiddev) == (uint)PRODUCT_ID;
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

        public static byte[] CreateBuffer(Report report)
        {
            var buffer = new byte[report.Length];
            buffer[0] = report.ReportID;
            return buffer;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}