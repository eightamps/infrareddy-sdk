using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrareddy
{
    public class Reddy : IDisposable
    {
        public Version SoftwareVersion { get; private set; }
        public Version HardwareVersion { get { return hiddev.ReleaseNumber; } }

        protected Reddy(HidStream hidStream)
        {
            this.stream = hidStream;
            this.reports = new Dictionary<HidUsage.EightAmps, Report>();
            this.rollingTag = 0;

            var reportDescriptor = hiddev.GetReportDescriptor();
            var deviceItem = reportDescriptor.DeviceItems.First();
            var freports = deviceItem.FeatureReports;
            var oreports = deviceItem.OutputReports;
            var ireports = deviceItem.InputReports;


            if (false)
            {
                this.verReport = freports.First(r => r.GetAllUsages().Contains((uint)HidUsage.GenericDevice.SoftwareVersion));
                reports.Add(HidUsage.EightAmps.ReddyResetCmdReport, oreports.First(r => r.GetAllUsages().Contains((uint)HidUsage.EightAmps.ReddyResetCmdReport)));
                reports.Add(HidUsage.EightAmps.ReddyEncodeCmdReport, oreports.First(r => r.GetAllUsages().Contains((uint)HidUsage.EightAmps.ReddyEncodeCmdReport)));
                reports.Add(HidUsage.EightAmps.ReddyDecodeCmdReport, oreports.First(r => r.GetAllUsages().Contains((uint)HidUsage.EightAmps.ReddyDecodeCmdReport)));
                reports.Add(HidUsage.EightAmps.ReddyStatusRspReport, ireports.First(r => r.GetAllUsages().Contains((uint)HidUsage.EightAmps.ReddyStatusRspReport)));
                reports.Add(HidUsage.EightAmps.ReddyDecodeRspReport, ireports.First(r => r.GetAllUsages().Contains((uint)HidUsage.EightAmps.ReddyDecodeRspReport)));
            }
            else
            {
                this.verReport = freports.First(r => r.GetAllUsages().Contains((uint)HidUsage.GenericDevice.Major));
                // hardcoding as fallback
                // could also consider sw version
                reports.Add(HidUsage.EightAmps.ReddyResetCmdReport, oreports.First(r => r.ReportID == 1));
                reports.Add(HidUsage.EightAmps.ReddyDecodeCmdReport, oreports.First(r => r.ReportID == 2));
                reports.Add(HidUsage.EightAmps.ReddyEncodeCmdReport, oreports.First(r => r.ReportID == 3));
                reports.Add(HidUsage.EightAmps.ReddyStatusRspReport, ireports.First(r => r.ReportID == 1));
                reports.Add(HidUsage.EightAmps.ReddyDecodeRspReport, ireports.First(r => r.ReportID == 2));
            }

            var buffer = verReport.CreateBuffer();
            stream.GetFeature(buffer);
            int major = 0, minor = 0, rev = 0;
            verReport.Read(buffer, 0, (dataValue) =>
            {
                var usage = (HidUsage.GenericDevice)dataValue.Usages.FirstOrDefault();
                int val = dataValue.GetLogicalValue();
                switch (usage)
                {
                    case HidUsage.GenericDevice.Major:
                        major = val;
                        break;
                    case HidUsage.GenericDevice.Minor:
                        minor = val;
                        break;
                    case HidUsage.GenericDevice.Revision:
                        rev = val;
                        break;
                    default:
                        break;
                }
            });
            SoftwareVersion = new Version(major, minor, rev);

            this.inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
            this.inputReceiver.Received += InputReceiver_Received;
            this.inputReceiver.Start(hidStream);
        }

        public static bool TryOpen(HidDevice hiddev, out Reddy reddy)
        {
            reddy = null;
            if (!hiddev.IsAspenDevice())
                return false;
            if (hiddev.GetApplicationUsage() != (uint)HidUsage.EightAmps.Reddy)
                return false;

            try
            {
                HidStream hidStream;
                if (!hiddev.TryOpen(out hidStream))
                    return false;
                hidStream.ReadTimeout = Timeout.Infinite;
                reddy = new Reddy(hidStream);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static Reddy First()
        {
            Reddy r = null;
            var device = DeviceList.Local.GetHidDevices().First(d => Reddy.TryOpen(d, out r));
            return r;
        }

        private HidDevice hiddev { get { return stream.Device; } }
        private HidStream stream;
        private Dictionary<HidUsage.EightAmps, Report> reports;
        private Report verReport;
        private ushort rollingTag;
        private HidDeviceInputReceiver inputReceiver;

        public void Dispose()
        {
            stream.Dispose();
        }

        private uint GetNewTag()
        {
            return ++rollingTag;
        }

        private event Action<Report, byte[]> ReportReceived = delegate { };

        private void InputReceiver_Received(object sender, EventArgs e)
        {
            var inputReportBuffer = new byte[hiddev.GetMaxInputReportLength()];
            Report report;
            while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
            {
                ReportReceived(report, inputReportBuffer);
            }
        }

        public enum RequestStatus
        {
            Success = 0,
            Busy,
            InvalidNotHex = 50,
            InvalidMalformed,
            UnsupportedProtocol,
            UnsupportedFrequency,
            InvalidSize,
            Invalid,
            Failure,
            SdkInternalError = 0x100,
            SdkStatusInvalidValue,
            SdkStatusMissingError,
            Timeout,
        }

        public Task<RequestStatus> Reset()
        {
            var tcs = new TaskCompletionSource<RequestStatus>();
            uint tag = GetNewTag();

            // when receiving a report, try to match it to this command
            Action<Report, byte[]> rxhandler = delegate (Report report, byte[] buffer)
            {
                lock (tcs)
                {
                    // make sure that these initial values are impossible to be set by the report data
                    uint rxtag = ~0u;
                    RequestStatus status = RequestStatus.SdkStatusMissingError;

                    // parse the data from the buffer
                    report.Read(buffer, 0, (dataValue) =>
                    {
                        var usage = (HidUsage.EightAmps)dataValue.Usages.FirstOrDefault();
                        int val = dataValue.GetLogicalValue();
                        switch (usage)
                        {
                            case HidUsage.EightAmps.TagId:
                                // simply save the tag
                                rxtag = (uint)val;
                                break;
                            case HidUsage.EightAmps.ReddyStatus:
                                // verify status enum validity
                                if (!Enum.IsDefined(typeof(RequestStatus), val))
                                {
                                    status = RequestStatus.SdkStatusInvalidValue;
                                }
                                else
                                {
                                    status = (RequestStatus)val;
                                }
                                break;
                            default:
                                // there should be no other values
                                break;
                        }
                    });

                    // when the response tag matches the command's, we have our result
                    if (tag == rxtag)
                    {
                        tcs.SetResult(status);
                        Monitor.Pulse(tcs);
                    }
                }
            };
            this.ReportReceived += rxhandler;

            // this is our async work for writing the command to the device
            Task.Run(() =>
            {
                try
                {
                    lock (tcs)
                    {
                        var report = reports[HidUsage.EightAmps.ReddyResetCmdReport];
                        var buffer = report.CreateBuffer();

                        report.Write(buffer, 0, (buf, bitOffset, dataItem, indexOfDataItem) =>
                        {
                            var usage = (HidUsage.EightAmps)dataItem.Usages.GetAllValues().FirstOrDefault();
                            switch (usage)
                            {
                                case HidUsage.EightAmps.TagId:
                                    dataItem.WriteRaw(buf, bitOffset, 0, tag);
                                    break;
                                default:
                                    break;
                            }
                        });
                        stream.Write(buffer);

                        // TODO: tune timeout
                        int responseTimeout = 500;

                        // if the receiver side doesn't signal in time, we time out
                        if (!Monitor.Wait(tcs, responseTimeout))
                        {
                            tcs.SetResult(RequestStatus.Timeout);
                        }
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    this.ReportReceived -= rxhandler;
                }
            });
            return tcs.Task;
        }

        public Task<RequestStatus> Encode(InfraredSignal signal, uint numberOfRepeats = 0, uint repeatIntervalMs = 1000)
        {
            var tcs = new TaskCompletionSource<RequestStatus>();
            uint tag = GetNewTag();

            // when receiving a report, try to match it to this command
            Action<Report, byte[]> rxhandler = delegate (Report report, byte[] buffer)
            {
                lock (tcs)
                {
                    // make sure that these initial values are impossible to be set by the report data
                    uint rxtag = ~0u;
                    RequestStatus status = RequestStatus.SdkStatusMissingError;

                    // parse the data from the buffer
                    report.Read(buffer, 0, (dataValue) =>
                    {
                        var usage = (HidUsage.EightAmps)dataValue.Usages.FirstOrDefault();
                        int val = dataValue.GetLogicalValue();
                        switch (usage)
                        {
                            case HidUsage.EightAmps.TagId:
                                // simply save the tag
                                rxtag = (uint)val;
                                break;
                            case HidUsage.EightAmps.ReddyStatus:
                                // verify status enum validity
                                if (!Enum.IsDefined(typeof(RequestStatus), val))
                                {
                                    status = RequestStatus.SdkStatusInvalidValue;
                                }
                                else
                                {
                                    status = (RequestStatus)val;
                                }
                                break;
                            default:
                                // there should be no other values
                                break;
                        }
                    });

                    // when the response tag matches the command's, we have our result
                    if (tag == rxtag)
                    {
                        tcs.SetResult(status);
                        Monitor.Pulse(tcs);
                    }
                }
            };
            this.ReportReceived += rxhandler;

            // this is our async work for writing the command to the device
            Task.Run(() =>
            {
                try
                {
                    lock (tcs)
                    {
                        var report = reports[HidUsage.EightAmps.ReddyEncodeCmdReport];
                        var buffer = report.CreateBuffer();
                        var rawdata = signal.ToRaw();

                        report.Write(buffer, 0, (buf, bitOffset, dataItem, indexOfDataItem) =>
                        {
                            var usage = (HidUsage.EightAmps)dataItem.Usages.GetAllValues().FirstOrDefault();
                            switch (usage)
                            {
                                case HidUsage.EightAmps.TagId:
                                    dataItem.WriteRaw(buf, bitOffset, 0, tag);
                                    break;
                                case HidUsage.EightAmps.ReddyProtocolType:
                                    dataItem.WriteRaw(buf, bitOffset, 0, (uint)signal.GetProtocol());
                                    break;
                                case HidUsage.EightAmps.ReddyProtocolDataLength:
                                    dataItem.WriteRaw(buf, bitOffset, 0, (uint)rawdata.Length);
                                    break;
                                case HidUsage.EightAmps.ReddyEncodeRepeats:
                                    dataItem.WriteRaw(buf, bitOffset, 0, numberOfRepeats);
                                    break;
                                case HidUsage.EightAmps.ReddyEncodeInterval:
                                    dataItem.WriteRaw(buf, bitOffset, 0, repeatIntervalMs);
                                    break;
                                case HidUsage.EightAmps.ReddyProtocolData:
                                    // TODO: check if rawdata can fit in report
                                    Array.Copy(rawdata, 0, buf, bitOffset / 8, rawdata.Length);
                                    break;
                                default:
                                    break;
                            }
                        });
                        stream.Write(buffer);

                        // TODO: tune timeout
                        int responseTimeout = 500 + (int)numberOfRepeats * (int)repeatIntervalMs;

                        // if the receiver side doesn't signal in time, we time out
                        if (!Monitor.Wait(tcs, responseTimeout))
                        {
                            tcs.SetResult(RequestStatus.Timeout);
                        }
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    this.ReportReceived -= rxhandler;
                }
            });
            return tcs.Task;
        }

        public class DecodeResponse
        {
            public RequestStatus Status { get; private set; }
            public InfraredSignal Signal { get; private set; }

            public DecodeResponse(RequestStatus status, InfraredSignal signal)
            {
                this.Status = status;
                this.Signal = signal;
            }
        }

        public Task<DecodeResponse> Decode(InfraredSignal.ProtocolType protocol, uint timeoutMs = 1000)
        {
            var tcs = new TaskCompletionSource<DecodeResponse>();
            uint tag = GetNewTag();

            // when receiving a report, try to match it to this command
            Action<Report, byte[]> rxhandler = delegate (Report report, byte[] buffer)
            {
                lock (tcs)
                {
                    // make sure that these initial values are impossible to be set by the report data
                    uint rxtag = ~0u;
                    RequestStatus status = RequestStatus.SdkStatusMissingError;
                    int dlength = -1, dindex = 0;
                    byte[] rawdata = null;

                    // parse the data from the buffer
                    report.Read(buffer, 0, (dataValue) =>
                    {
                        var usage = (HidUsage.EightAmps)dataValue.Usages.FirstOrDefault();
                        int val = dataValue.GetLogicalValue();
                        switch (usage)
                        {
                            case HidUsage.EightAmps.TagId:
                                // simply save the tag
                                rxtag = (uint)val;
                                break;
                            case HidUsage.EightAmps.ReddyStatus:
                                // verify status enum validity
                                if (!Enum.IsDefined(typeof(RequestStatus), val))
                                {
                                    status = RequestStatus.SdkStatusInvalidValue;
                                }
                                else
                                {
                                    status = (RequestStatus)val;
                                }
                                break;
                            case HidUsage.EightAmps.ReddyProtocolDataLength:
                                dlength = val;
                                rawdata = new byte[dlength];
                                break;
                            case HidUsage.EightAmps.ReddyProtocolData:
                                if (dindex < dlength)
                                {
                                    rawdata[dindex] = (byte)val;
                                    dindex++;
                                }
                                break;
                            case HidUsage.EightAmps.ReddyProtocolType:
                                // TODO: maybe cross-check with input protocol?
                                break;
                            default:
                                break;
                        }
                    });

                    // when the response tag matches the command's, we have our result
                    if (tag == rxtag)
                    {
                        tcs.SetResult(new DecodeResponse(status, InfraredSignal.FromRaw(rawdata, protocol)));
                        Monitor.Pulse(tcs);
                    }
                }
            };
            this.ReportReceived += rxhandler;

            // this is our async work for writing the command to the device
            Task.Run(() =>
            {
                try
                {
                    lock (tcs)
                    {
                        var report = reports[HidUsage.EightAmps.ReddyDecodeCmdReport];
                        var buffer = report.CreateBuffer();

                        report.Write(buffer, 0, (buf, bitOffset, dataItem, indexOfDataItem) =>
                        {
                            var usage = (HidUsage.EightAmps)dataItem.Usages.GetAllValues().FirstOrDefault();
                            switch (usage)
                            {
                                case HidUsage.EightAmps.TagId:
                                    dataItem.WriteRaw(buf, bitOffset, 0, tag);
                                    break;
                                case HidUsage.EightAmps.ReddyProtocolType:
                                    dataItem.WriteRaw(buf, bitOffset, 0, (uint)protocol);
                                    break;
                                case HidUsage.EightAmps.ReddyDecodeTimeout:
                                    dataItem.WriteRaw(buf, bitOffset, 0, (uint)timeoutMs);
                                    break;
                                default:
                                    break;
                            }
                        });
                        stream.Write(buffer);

                        // TODO: tune timeout
                        int responseTimeout = 500 + (int)timeoutMs;

                        // if the receiver side doesn't signal in time, we time out
                        if (!Monitor.Wait(tcs, responseTimeout))
                        {
                            tcs.SetResult(new DecodeResponse(RequestStatus.Timeout, null));
                        }
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    this.ReportReceived -= rxhandler;
                }
            });
            return tcs.Task;
        }
    }
}
