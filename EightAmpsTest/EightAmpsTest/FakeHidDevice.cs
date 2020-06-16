using HidSharp;
using System;

namespace EightAmpsTest
{
    class FakeHidDevice : HidDevice
    {
        public FakeHidDevice()
        {
        }

        public override string DevicePath => throw new NotImplementedException();

        public override int ProductID => throw new NotImplementedException();

        public override int ReleaseNumberBcd => throw new NotImplementedException();

        [Obsolete]
        public override int ProductVersion => base.ProductVersion;

        public override int VendorID => throw new NotImplementedException();

        [Obsolete]
        public override string Manufacturer => base.Manufacturer;

        [Obsolete]
        public override string ProductName => base.ProductName;

        [Obsolete]
        public override string SerialNumber => base.SerialNumber;

        [Obsolete]
        public override int MaxInputReportLength => base.MaxInputReportLength;

        [Obsolete]
        public override int MaxOutputReportLength => base.MaxOutputReportLength;

        [Obsolete]
        public override int MaxFeatureReportLength => base.MaxFeatureReportLength;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string GetFileSystemName()
        {
            throw new NotImplementedException();
        }

        public override string GetFriendlyName()
        {
            return base.GetFriendlyName();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string GetManufacturer()
        {
            throw new NotImplementedException();
        }

        public override int GetMaxFeatureReportLength()
        {
            throw new NotImplementedException();
        }

        public override int GetMaxInputReportLength()
        {
            throw new NotImplementedException();
        }

        public override int GetMaxOutputReportLength()
        {
            throw new NotImplementedException();
        }

        public override string GetProductName()
        {
            throw new NotImplementedException();
        }

        public override byte[] GetRawReportDescriptor()
        {
            return base.GetRawReportDescriptor();
        }

        public override string GetSerialNumber()
        {
            throw new NotImplementedException();
        }

        public override string[] GetSerialPorts()
        {
            return base.GetSerialPorts();
        }

        public override bool HasImplementationDetail(Guid detail)
        {
            return base.HasImplementationDetail(detail);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override string GetStreamPath(OpenConfiguration openConfig)
        {
            return base.GetStreamPath(openConfig);
        }

        protected override DeviceStream OpenDeviceAndRestrictAccess(OpenConfiguration openConfig)
        {
            return base.OpenDeviceAndRestrictAccess(openConfig);
        }

        protected override DeviceStream OpenDeviceDirectly(OpenConfiguration openConfig)
        {
            throw new NotImplementedException();
        }
    }
}
