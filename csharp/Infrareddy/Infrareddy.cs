using System.Runtime.InteropServices;
using System.Text;
using System;
using HidSharp;
using System.Linq;

namespace EightAmps
{
    public class Infrareddy
    {
        private const int IR_READ_TIMEOUT_MS = 30000;
        public const UInt16 ASPEN_VENDOR_ID = 0x0483;
        public const UInt16 ASPEN_PRODUCT_ID = 0xa367;
        public const UInt16 MAPLE_VENDOR_ID = 0x335e;
        public const UInt16 MAPLE_PRODUCT_ID = 0x8a01;
        public const uint INFRAREDDY_APPLICATION_USAGE_ID = 0xff8a0002;

        public const UInt16 IR_ENVELOPE_SIZE = (4096 - 1);
        public const UInt16 IR_ENCODE_DATA_SIZE = (IR_ENVELOPE_SIZE - (1 + 2 + 4 + 2));
        public const UInt16 IR_DECODE_DATA_SIZE = (IR_ENVELOPE_SIZE - (1 + 2 + 4 + 4 + 2));

        // Report Identifiers
        public const byte OUT_ID_DECODE_CMD = 2;
        public const byte OUT_ID_ENCODE_CMD = 3;
        public const byte OUT_ID_POWER_CMD = 4;

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
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct PowerCmdReportType
        {
            public byte id;
            public UInt16 tag;
            public byte isHighPower;
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
            public Int32 type;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct DecodeCmdResponseType
        {
            public byte id;
            public UInt16 tag;
            public Int32 status;
            public Int32 type;
            public UInt16 len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IR_DECODE_DATA_SIZE)]
            public byte[] data;
        }

        public enum RequestStatus
        {
            IR_SUCCESS = 0,
            // Response codes from 1-49 are Retryable responses.
            IR_IS_BUSY,
            // Response codes > 50 should not be retried automatically.
            IR_INVALID_NOT_HEX = 50,
            IR_INVALID_MALFORMED,
            IR_UNSUPPORTED_FORMAT,
            IR_UNSUPPORTED_PROTOCOL,
            IR_UNSUPPORTED_FREQUENCY,
            IR_TIMEOUT_EXCEEDED,
            IR_INVALID_SIZE,
            IR_INVALID,
            IR_FAILURE,
        }

        public enum ProtocolType : Int32
        {
            IR_PROTOCOL_PRONTO = 0,
            IR_PROTOCOL_SIRC,
            IR_PROTOCOL_NEC,
            IR_PROTOCOL_RC5,
            IR_PROTOCOL_RAW,
            REDDY_PROT_LAST = IR_PROTOCOL_RAW,
        }

        public struct DecodeResponse
        {
            public RequestStatus status;
            public string payload;
        }

        public static byte Repeat = 0x01;
        public static byte NoRepeat = 0x00;
        public Version SoftwareVersion { get; private set; }
        public Version HardwareVersion { get { return null; } } // return hiddev.ReleaseNumber; } }

        public bool IsHighPower {
          get { return this.isHighPower; }
          set { 
            if (value != this.isHighPower) {
              this.isHighPower = value;
              this.isHighPowerChanged = true;
            }
          }
        }

        public delegate void CompleteHandler(RequestStatus status);

        public static Infrareddy First()
        {
            var device = DeviceList.Local.GetHidDevices().Where(d => Infrareddy.IsInfrareddy(d)).FirstOrDefault();
            return device == null ? null : new Infrareddy(device);
        }

        /**
         * Return true if the provided HID Device looks like Infrareddy hardware.
         * Specifically, it should have the correct Vendor ID, Product ID and declare the expected
         * USB application usage.
         */
        private static bool IsInfrareddy(HidDevice hiddev)
        {
            return (Infrareddy.IsAspenDevice(hiddev) ||
                Infrareddy.IsMapleDevice(hiddev)) &&
                Infrareddy.GetApplicationUsage(hiddev) == INFRAREDDY_APPLICATION_USAGE_ID;
        }

        public static uint GetApplicationUsage(HidDevice device)
        {
            return device == null ? 0 : device.GetReportDescriptor()
                .DeviceItems.FirstOrDefault()
                .Usages.GetAllValues().FirstOrDefault();
        }

        public static bool IsAspenDevice(HidDevice device)
        {
            return device != null && 
                device.VendorID == ASPEN_VENDOR_ID &&
                device.ProductID == ASPEN_PRODUCT_ID;
        }

        public static bool IsMapleDevice(HidDevice device)
        {
            return device != null && 
                device.VendorID == MAPLE_VENDOR_ID &&
                device.ProductID == MAPLE_PRODUCT_ID;
        }

        private HidStream stream;
        private bool isHighPower;
        private bool isHighPowerChanged;

        public Infrareddy(HidDevice device)
        {
            this.stream = OpenStream(device);
            // TODO(lbayes): Get Value from Settings Service
            this.IsHighPower = true;

            // TODO(lbayes): Deal with these events.
            // device.Inserted += DeviceAttachedHandler;
            // device.Removed += DeviceRemovedHandler;
            // device.MonitorDeviceEvents = true;
        }

        private HidStream OpenStream(HidDevice device)
        {
            if (device.TryOpen(out stream))
            {
                stream.ReadTimeout = IR_READ_TIMEOUT_MS;
                return stream;
            }
            return null;
        }

        public void Dispose()
        {
            try
            {
                stream.Close();
            } catch (Exception err) {
                Console.WriteLine(err);
            };
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

        private byte[] AsciiToBytes(string command)
        {
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] ExpandArrayToWireSize(byte[] prontoBytes)
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
         * Create a new Infrared Power Report
         */
        private PowerCmdReportType CreatePowerReport(bool isHigh)
        {
          return new PowerCmdReportType
          {
              id = OUT_ID_POWER_CMD,
              tag = Infrareddy.NextRequestTag(),
              isHighPower = Convert.ToByte(isHigh ? 1 : 0),
          };
        }

        public EncodeCmdReportType CreateEncodeReport(string data, UInt16 requestTag, ProtocolType type)
        {
            data = data.Substring(0, Math.Min(data.Length, IR_ENCODE_DATA_SIZE));
            var prontoBytes = AsciiToBytes(data);
            var wireBytes = ExpandArrayToWireSize(prontoBytes);

            return new EncodeCmdReportType
            {
                id = OUT_ID_ENCODE_CMD,
                tag = requestTag,
                len = (UInt16)prontoBytes.Length,
                type = (Int32)type,
                data = wireBytes,
            };
        }

        public RequestStatus Encode(string data, byte isRepeat, Infrareddy.ProtocolType type)
        {
            // Set the IR Power if it has been changed since last encode.
            UpdateIrPowerIfNecessary();

            if (data.Length > IR_ENCODE_DATA_SIZE)
            {
                throw new InvalidOperationException("EmitPronto called with IR code that is too long.");
            }

            var requestTag = Infrareddy.NextRequestTag();
            // Send the report over the wire.
            var report = CreateEncodeReport(data, requestTag, type);
            var writeBytes = StructureToByteArray(report);
            // Get the response report from the wire.

            if (Convert.ToBoolean(isRepeat))
            {
                Console.WriteLine("WARNING: isRepeat sent to Encode, but not forwarded to device");
            }

            try
            {
                stream.Write(writeBytes);
                // Don't let result failure block the IR Worker thread
                var readResponse = stream.Read();
                object responseObj = new StatusRspReportType { };
                ByteArrayToStructure(readResponse, ref responseObj);
                StatusRspReportType response = (StatusRspReportType)responseObj;
                return (RequestStatus)response.status;
            }
            catch (TimeoutException)
            {
                return RequestStatus.IR_TIMEOUT_EXCEEDED;
            }
        }

        /**
         * Update the Infrared Power output if it has changed since the last time
         * we successfully notified the connected client.
         *
         * NOTE(lbayes): This call will be ignored by legacy clients that do not
         * declare support for this Report ID (e.g., Legacy AT-12's)
         */
        private RequestStatus UpdateIrPowerIfNecessary()
        {
            if (isHighPowerChanged)
            {
                try
                {
                    var report = CreatePowerReport(this.IsHighPower);
                    var writeBytes = StructureToByteArray(report);
                    stream.Write(writeBytes);
                    this.isHighPowerChanged = false;

                    var readResponse = stream.Read();
                    object responseObj = new StatusRspReportType { };
                    ByteArrayToStructure(readResponse, ref responseObj);
                    StatusRspReportType response = (StatusRspReportType)responseObj;

                    this.IsHighPower = !this.IsHighPower;
                    return (RequestStatus)response.status;
                }
                catch (Exception)
                {
                    // Legacy Aspen systems do not accept this report, so will fail.
                    // Legacy Maple systems do not accept his report, so will fail.
                    // The default value on systems that support this report is LOW POWER, so failure to change is safe.
                    return RequestStatus.IR_FAILURE;
                }
            }

            return RequestStatus.IR_SUCCESS;
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
            return Encode(prontoStr, isRepeat, ProtocolType.IR_PROTOCOL_PRONTO);
        }

        /**
         * Begin listening for IR signals and return the requested encoding.
         */
        public DecodeResponse Decode(Infrareddy.ProtocolType type)
        {
            Infrareddy.NextRequestTag();
            var requestTag = Infrareddy.NextRequestTag();
            var command = new DecodeCmdReportType
            {
                id = OUT_ID_DECODE_CMD,
                tag = requestTag,
                type = (Int32)type,
            };
            Console.WriteLine("Requesting Decode from device with: {0}", type);
            try
            {
                stream.Write(StructureToByteArray(command));
                Console.WriteLine("Attempt to read Decoded data from device");
                var readResponse = stream.Read();
                Console.WriteLine("Read complete!");
                object responseObj = new DecodeCmdResponseType { };
                ByteArrayToStructure(readResponse, ref responseObj);
                var responseStruct = (DecodeCmdResponseType)responseObj;
                return new DecodeResponse
                {
                    status = (RequestStatus)responseStruct.status,
                    payload = BytesToString(responseStruct.data),
                };
            }
            catch (TimeoutException)
            {
                return new DecodeResponse
                {
                    status = RequestStatus.IR_TIMEOUT_EXCEEDED,
                    payload = "Read Failure: Due to Timeout exceeded, try shorter presses",
                };
            }
        }

        /**
         * Tell the hardware to begin listening for IR signals and to return
         * them in Pronto format.
         */
        public DecodeResponse DecodePronto()
        {
            return Decode(ProtocolType.IR_PROTOCOL_PRONTO);
        }

        /**
         * Tell the hardware to begin listening for IR signals and to return
         * them in RAW (8A format).
         */
        public DecodeResponse DecodeRaw()
        {
            return Decode(ProtocolType.IR_PROTOCOL_RAW);
        }

        private string BytesToString(byte[] bytes)
        {
            var bigStr = Encoding.ASCII.GetString(bytes);
            string result = "";
            for (var i = 0; i < bigStr.Length; i++)
            {
                if (bytes[i] == 0)
                {
                    break;
                }
                result += bigStr[i];
            }

            return result;
        }
    }
}
