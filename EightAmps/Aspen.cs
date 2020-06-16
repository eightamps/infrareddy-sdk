using System.Linq;
using HidSharp;
using HidSharp.Reports;
using LibUsbDotNet;

namespace EightAmps
{

    public static class Aspen
    {
        public static bool IsAspenDevice(this HidDevice hiddev)
        {
            if (hiddev == null)
                return false;
            if (hiddev.VendorID != 0x0483)
                return false;
            if (hiddev.ProductID != 0xa367)
                return false;
            return true;
        }
        public static bool IsAspenDevice(this UsbDevice usbdev)
        {
            if (usbdev == null)
                return false;
            if ((ushort)usbdev.Info.Descriptor.VendorID != 0x0483)
                return false;
            if ((ushort)usbdev.Info.Descriptor.ProductID != 0xa367)
                return false;
            return true;
        }

        public static uint GetApplicationUsage(this HidDevice hiddev)
        {
            if (hiddev == null)
                return 0;
            var reportDescriptor = hiddev.GetReportDescriptor();
            var ditem = reportDescriptor.DeviceItems.FirstOrDefault();
            return ditem.Usages.GetAllValues().FirstOrDefault();
        }

        public static byte[] CreateBuffer(this Report report)
        {
            byte[] buffer = new byte[report.Length];
            buffer[0] = report.ReportID;
            return buffer;
        }
    }
}
