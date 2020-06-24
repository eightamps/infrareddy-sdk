using System.Runtime.InteropServices;
using System.Text;
using System;
using HidLibrary;
using System.Linq;

namespace EightAmps
{
    public class Infrareddy
    {
        public const UInt16 ASPEN_VENDOR_ID = 0x0483;
        public const UInt16 ASPEN_PRODUCT_ID = 0xa367;
        public const uint ASPEN_REPORT_ID = 0xff8a0002;

        public const UInt16 IR_ENVELOPE_SIZE = (4096 - 1);
        public const UInt16 IR_ENCODE_DATA_SIZE = (IR_ENVELOPE_SIZE - (1 + 2 + 4 + 2));
        public const UInt16 IR_DECODE_DATA_SIZE = (IR_ENVELOPE_SIZE - (1 + 2 + 4 + 4 + 2));

        // Report Identifiers
        public const byte OUT_ID_DECODE_CMD = 2;
        public const byte OUT_ID_ENCODE_CMD = 3;
        public const byte IN_ID_STATUS_RSP = 1;

        private static UInt16 RequestTag = 223;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct EncodeCmdReportType
        {
            public byte id;
            public UInt16 tag;
            public Int32 type;
            public UInt16 len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IR_ENCODE_DATA_SIZE)]
            public byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 7)]
        public struct StatusRspReportType
        {
            public byte id;
            public UInt16 tag;
            public Int32 status;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct DecodeCmdReportType
        {
            public byte id;
            public UInt16 tag;
            public UInt16 timeoutMs;
            public ProtocolType type;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct DecodeCmdResponseType
        {
            public byte id;
            public UInt16 tag;
            public Int32 status;
            public ProtocolType type;
            public UInt16 len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IR_DECODE_DATA_SIZE)]
            public byte[] data;
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
            IR_FAILURE,
        }

        public enum ProtocolType : Int32
        {
            REDDY_PROT_PRONTO = 0,
            REDDY_PROT_SIRC,
            REDDY_PROT_NEC,
            REDDY_PROT_RAW,
            REDDY_PROT_RC5,
            REDDY_PROT_LAST = REDDY_PROT_PRONTO,  // We only support PRONTO for now.
        }

        public struct DecodeProntoResponse
        {
            public RequestStatus status;
            public string payload;
        }

        public static byte Repeat = 0x01;
        public static byte NoRepeat = 0x00;
        public Version SoftwareVersion { get; private set; }
        public Version HardwareVersion { get { return null; } } // return hiddev.ReleaseNumber; } }
        public delegate void CompleteHandler(RequestStatus status);

        public static Infrareddy First()
        {
            var device = HidDevices.Enumerate(ASPEN_VENDOR_ID, ASPEN_PRODUCT_ID).FirstOrDefault();
            if (device != null)
            {
                return new Infrareddy(device);
            }

            return null;
        }

        private HidDevice device;
        
        public Infrareddy(HidDevice device)
        {
            this.device = device;
            device.OpenDevice();
            // TODO(lbayes): Deal with these events.
            // device.Inserted += DeviceAttachedHandler;
            // device.Removed += DeviceRemovedHandler;
            // device.MonitorDeviceEvents = true;
        }

        public void Dispose()
        {
            device.CloseDevice();
        }

        byte[] StructureToByteArray(object obj)
        {
            var len = Marshal.SizeOf(obj);
            var arr = new byte[len];
            var ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        void ByteArrayToStructure(byte[] bytearray, ref object obj)
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
        public static UInt16 NextRequestTag()
        {
            return RequestTag++;
        }

        private byte[] ProntoStringToBytes(string command)
        {
            return Encoding.ASCII.GetBytes(command);
        }

        // Convert the C# string command to Byte array.
        public byte[] ProntoBytesToWireBytes(byte[] prontoBytes)
        {
            var bytes = new byte[IR_ENCODE_DATA_SIZE];
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
            return Encoding.ASCII.GetString(report.data);
        }

        public EncodeCmdReportType ProntoToReport(string pronto, UInt16 requestTag)
        {
            // TODO(lbayes): DONOTSUBMIT - truncate data to fit in testing values
            pronto = pronto.Substring(0, Math.Min(pronto.Length, IR_ENCODE_DATA_SIZE));
            var prontoBytes = ProntoStringToBytes(pronto);
            var wireBytes = ProntoBytesToWireBytes(prontoBytes);

            return new EncodeCmdReportType
            {
                id = OUT_ID_ENCODE_CMD,
                tag = requestTag,
                len = (UInt16)prontoBytes.Length,
                type = (Int32)0,
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
        public RequestStatus EncodePronto(string prontoStr, byte isRepeat)
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
            device.Write(writeBytes);

            // DONOTSUBMIT
            var readResponse = device.Read();
            if (readResponse.Status != HidDeviceData.ReadStatus.Success)
            {
                Console.WriteLine("Read failed");
            }

            object responseObj = new StatusRspReportType { };
            ByteArrayToStructure(readResponse.Data, ref responseObj);
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
                id = OUT_ID_DECODE_CMD,
                tag = requestTag,
            };
            Console.WriteLine("Requesting Decode from device");
            device.Write(StructureToByteArray(command));

            Console.WriteLine("Attempt to read Decoded data from device");
            var readResponse = device.Read();
            Console.WriteLine("Read complete!");

            if (readResponse.Status == HidDeviceData.ReadStatus.Success)
            {
                object responseObj = new DecodeCmdResponseType { };
                ByteArrayToStructure(readResponse.Data, ref responseObj);
                var responseStruct = (DecodeCmdResponseType)responseObj;
                var status = (RequestStatus)responseStruct.status;
                var payload = bytesToString(responseStruct.data);

                return new DecodeProntoResponse
                {
                    status = status,
                    payload = payload,
                };
            }
            else
            {
                Console.WriteLine("FAILED to get decode response");
                // TODO(lbayes): Add failure response.
                return new DecodeProntoResponse
                {
                    status = RequestStatus.IR_FAILURE,
                };
            }
        }

        private string bytesToString(Byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}