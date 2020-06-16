using System;
using System.Collections.ObjectModel;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace DfuBooter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting search for USB Device");
            // Dump all devices and descriptor information to console output.
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                UsbDevice MyUsbDevice;

                if ((usbRegistry.Vid != 0x0483) || (usbRegistry.Pid != 0xa367))
                    continue;

                if (usbRegistry.Open(out MyUsbDevice))
                {
                    Console.WriteLine(MyUsbDevice.Info.ToString());
                    
                    for (int iConfig = 0; iConfig < MyUsbDevice.Configs.Count; iConfig++)
                    {
                        UsbConfigInfo configInfo = MyUsbDevice.Configs[iConfig];
                        Console.WriteLine(configInfo.ToString());

                        ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
                        for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                        {
                            UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                            Console.WriteLine(interfaceInfo.ToString());

                            if (interfaceInfo.CustomDescriptors.Count != 1)
                            {
                                Console.WriteLine("Would have bailed here because CustomDescriptors is != 1:");
                                Console.WriteLine(interfaceInfo.CustomDescriptors.Count.ToString());
                                // continue;
                            }

                            Console.WriteLine("Sending ControlTransfer message now");
                            UsbSetupPacket s = new UsbSetupPacket(0x21, 0, 0, 0, 0);
                            int len;
                            MyUsbDevice.ControlTransfer(ref s, null, 0, out len);
                            Console.WriteLine("Bytes Sent:");
                            Console.WriteLine(len.ToString());
                        }
                    }
                }
            }


            // Free usb resources.
            // This is necessary for libusb-1.0 and Linux compatibility.
            UsbDevice.Exit();
        }
    }
}
