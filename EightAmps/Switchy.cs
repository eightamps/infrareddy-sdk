using System;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports;

namespace EightAmps
{
    public class Switchy : IDisposable
    {
        #region configuration classes
        public abstract class Conf
        {
            protected enum Type : byte
            {
                Keyboard = 0,
                Mouse,
                Consumer,
                Gamepad,
            }
            private Type type;
            protected short[] actions;
            protected Conf(Type type)
            {
                this.type = type;
                this.actions = new short[2] { 0, 0 };
            }
            public byte[] Encode(Report report)
            {
                var buffer = report.CreateBuffer();
                report.Write(buffer, 0, (buf, bitOffset, dataItem, indexOfDataItem) =>
                {
                    if(dataItem.Usages.ContainsValue((uint)ProductIds.SwitchySelector))
                    {
                        dataItem.WriteRaw(buf, bitOffset, 0, (uint)type);
                    }
                    else if (dataItem.Usages.ContainsValue((uint)ProductIds.SwitchyAction))
                    {
                        dataItem.WriteRaw(buf, bitOffset, 0, (uint)actions[indexOfDataItem - 1]);
                    }
                });
                return buffer;
            }
            static public Conf Decode(Report report, byte[] buffer)
            {
                Type type;
                short[] actions = new short[2];
                int i = 0;
                unchecked
                {
                    // initialize to invalid
                    type = (Type)(-1);
                }
                report.Read(buffer, 0, (dataValue) =>
                {
                    uint usage = dataValue.Usages.FirstOrDefault();
                    int val = dataValue.GetLogicalValue();
                    if (usage == (uint)ProductIds.SwitchySelector)
                    {
                        if (!Enum.IsDefined(typeof(Type), (byte)val))
                            throw new ArgumentOutOfRangeException("Not a recognized Conf.Type");
                        type = (Type)val;
                    }
                    else if (usage == (uint)ProductIds.SwitchyAction)
                    {
                        actions[i] = (short)val;
                        i++;
                    }
                });
                Conf conf;
                switch (type)
                {
                    case Type.Keyboard:
                        conf = new KeyboardConf();
                        break;
                    case Type.Mouse:
                        conf = new MouseConf();
                        break;
                    case Type.Consumer:
                        conf = new ConsumerConf();
                        break;
                    case Type.Gamepad:
                        conf = new GamepadConf();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Not a recognized Conf.Type");
                }
                Array.Copy(actions, conf.actions, i);
                return conf;
            }
            protected abstract string GetActionName(short action);
            public override string ToString()
            {
                return GetType().Name + "{" + GetActionName(actions[0])
                                      + ", " + GetActionName(actions[1])
                                      + "}";
            }
        }

        public class KeyboardConf : Conf
        {
            public enum Key : short
            {
                KNone = 0,
                KErrorRollOver,
                KErrorPOSTFail,
                KErrorUndefined,
                Ka,
                Kb,
                Kc,
                Kd,
                Ke,
                Kf,
                Kg,
                Kh,
                Ki,
                Kj,
                Kk,
                Kl,
                Km,
                Kn,
                Ko,
                Kp,
                Kq,
                Kr,
                Ks,
                Kt,
                Ku,
                Kv,
                Kw,
                Kx,
                Ky,
                Kz,
                K1,
                K2,
                K3,
                K4,
                K5,
                K6,
                K7,
                K8,
                K9,
                K0,
                KEnter,
                KEsc,
                KBackspace,
                KTab,
                KSpace,
                KMinusUnderscore,
                KEqualPlus,
                KLeftBrace,
                KRightBrace,
                KBackslashPipe,
                KHashTilde,
                KSemiColon,
                KApostrophe,
                KGrave,
                KCommaLessthan,
                KDotGreaterthan,
                KSlashQuestionmark,
                KCapsLock,
                KF1,
                KF2,
                KF3,
                KF4,
                KF5,
                KF6,
                KF7,
                KF8,
                KF9,
                KF10,
                KF11,
                KF12,
                KPrintscreenSysrq,
                KScrollLock,
                KPause,
                KInsert,
                KHome,
                KPageup,
                KDelete,
                KEnd,
                KPagedown,
                KRight,
                KLeft,
                KDown,
                KUp,
                KNumLock,
                KPSlash,
                KPAsterisk,
                KPMinus,
                KPPlus,
                KPEnter,
                KP1,
                KP2,
                KP3,
                KP4,
                KP5,
                KP6,
                KP7,
                KP8,
                KP9,
                KP0,
                KPDot,
                KLessGreaterthan,
                KApplication,
                KPower, /* = 0x66 */

                KOpen = 0x74,
                KHelp,
                KProps,
                KFront,
                KStop,
                KAgain,
                KUndo,
                KCut,
                KCopy,
                KPaste,
                KFind,
                KMute,
                KVolumeUp,
                KVolumeDown, /* = 0x81 */

                KLeftCtrl = 0xE0,
                KLeftShift,
                KLeftAlt,
                KLeftGUI,
                KRightCtrl,
                KRightShift,
                KRightAlt,
                KRightGUI,

                KMediaPlayPause = 0xE8,
                KMediaPrevious = 0xEA,
                KMediaNext,
                KMediaEject,
                KMediaVolumeUp,
                KMediaVolumeDown,
                KMediaMute,
                KMediaBack = 0xF1,
                KMediaForward,
                KMediaStop,

                // key modifiers
                MLeftCtrl = 0x100 | 0x01,
                MLeftShift = 0x100 | 0x02,
                MLeftAlt = 0x100 | 0x04,
                MLeftGUI = 0x100 | 0x08,
                MRightCtrl = 0x100 | 0x10,
                MRightShift = 0x100 | 0x20,
                MRightAlt = 0x100 | 0x40,
                MRightGUI = 0x100 | 0x80,
            }

            public KeyboardConf() : base(Type.Keyboard)
            {
            }

            public KeyboardConf SetKey(int switchNo, Key key)
            {
                this.actions[switchNo] = (short)key;
                return this;
            }

            protected override string GetActionName(short action)
            {
                return Enum.GetName(typeof(Key), (Key)action);
            }
        }

        public class MouseConf : Conf
        {
            public enum Button : byte
            {
                Left = 0,
                Right,
                Middle,
            }

            public MouseConf() : base(Type.Mouse)
            {
            }

            public MouseConf SetButton(int switchNo, Button button)
            {
                this.actions[switchNo] = (short)button;
                return this;
            }

            protected override string GetActionName(short action)
            {
                return Enum.GetName(typeof(Button), (Button)action);
            }
        }

        public class ConsumerConf : Conf
        {
            // there's a bunch more in HID spec
            // full list at https://github.com/IntergatedCircuits/hid-usage-tables/blob/master/pages/000c-consumer.txt
            public enum Key : short
            {
                KNone = 0,
                KPower = 0x030,
                KFactoryReset = 0x031,
                KSleep = 0x032,

                KSelectTV = 0x089,
                KSelectWWW = 0x08a,
                KSelectDVD = 0x08b,
                KSelectCD = 0x091,
                KSelectSat = 0x098,
                KChannelUp = 0x09c,
                KChannelDown = 0x09d,

                KPlay = 0x0b0, // consider KPlayPause instead
                KPause = 0x0b1, // consider KPlayPause instead
                KNextTrack = 0x0b5,
                KPrevTrack = 0x0b6,
                KStop = 0x0b7,
                KShuffle = 0x0b9,
                KRepeat = 0x0bc,
                KPlayPause = 0x0cd,
                KVoiceCommand = 0x0cf,

                KVolumeUp = 0x0e9,
                KVolumeDown = 0x0ea,
                KBalanceRight = 0x150,
                KBalanceLeft = 0x151,
                KBassUp = 0x152,
                KBassDown = 0x153,
                KTrebleUp = 0x154,
                KTrebleDown = 0x155,

                KNextApp = 0x1a3,
                KPreviousApp = 0x1a4,
            }

            public ConsumerConf() : base(Type.Consumer)
            {
            }

            public ConsumerConf SetKey(int switchNo, Key key)
            {
                this.actions[switchNo] = (short)key;
                return this;
            }

            protected override string GetActionName(short action)
            {
                return Enum.GetName(typeof(Key), (Key)action);
            }
        }

        public class GamepadConf : Conf
        {
            // http://eggbox.fantomfactory.org/pods/afGamepad/doc/
            public enum Button : byte
            {
                A = 0, // Face button 1
                B, // Face button 2
                X, // Face button 3
                Y, // Face button 4
                L1, // Left top shoulder
                R1, // Right top shoulder
                L2, // Left bottom shoulder
                R2, // Right bottom shoulder
                Select_Back,
                Start_Forward,
                LStickPress,
                RStickPress,
                DpadUp,
                DpadDown,
                DpadLeft,
                DpadRight,
            }

            public GamepadConf() : base(Type.Gamepad)
            {
            }

            public GamepadConf SetButton(int switchNo, Button button)
            {
                this.actions[switchNo] = (short)button;
                return this;
            }

            protected override string GetActionName(short action)
            {
                return Enum.GetName(typeof(Button), (Button)action);
            }
        }
        #endregion

        public Version SoftwareVersion { get; private set; }
        public Version HardwareVersion { get { return hiddev.ReleaseNumber; } }

        protected Switchy(HidStream hidStream)
        {
            this.stream = hidStream;
            var freports = hiddev.GetReportDescriptor().DeviceItems.First().FeatureReports;
            this.confReport = freports.First(r => r.GetAllUsages().Contains(0xFF8A0020u));
            this.verReport = freports.First(r => r.GetAllUsages().Contains(0x6002Du));

            var buffer = verReport.CreateBuffer();
            stream.GetFeature(buffer);
            int major = 0, minor = 0, rev = 0;
            verReport.Read(buffer, 0, (dataValue) =>
            {
                var usage = (EightAmps.GenericDevice)dataValue.Usages.FirstOrDefault();
                int val = dataValue.GetLogicalValue();
                switch (usage)
                {
                    case GenericDevice.Major:
                        major = val;
                        break;
                    case GenericDevice.Minor:
                        minor = val;
                        break;
                    case GenericDevice.Revision:
                        rev = val;
                        break;
                    default:
                        break;
                }
            });
            SoftwareVersion = new Version(major, minor, rev);
        }
        public static bool TryOpen(HidDevice hiddev, out Switchy switchy)
        {
            switchy = null;
            if (!hiddev.IsAspenDevice())
                return false;
            if (hiddev.GetApplicationUsage() != (uint)ProductIds.Switchy)
                return false;

            HidStream hidStream;
            if (!hiddev.TryOpen(out hidStream))
                return false;
            hidStream.ReadTimeout = Timeout.Infinite;
            switchy = new Switchy(hidStream);
            return true;
        }

        public int SwitchCount { get { return 2; } }

        public Conf Config
        {
            get
            {
                var buffer = confReport.CreateBuffer();
                stream.GetFeature(buffer);
                return Conf.Decode(confReport, buffer);
            }
            set
            {
                stream.SetFeature(value.Encode(confReport));
            }
        }

        private HidDevice hiddev { get { return stream.Device; } }
        private HidStream stream;
        private Report confReport;
        private Report verReport;

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
