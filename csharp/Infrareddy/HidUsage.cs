using System;
using System.Collections.Generic;
using System.Text;

namespace HidUsage
{
    public enum GenericDevice : uint
    {
        BackgroundControls = 0x00060001,
        BatteryStrength = 0x00060020,
        WirelessChannel,
        WirelessID,
        DiscoverWirelessControl,
        SecurityCodeCharacterEntered,
        SecurityCodeCharacterErased,
        SecurityCodeCleared,
        SequenceID,
        SequenceIDReset,
        RFSignalStrength,
        SoftwareVersion,
        ProtocolVersion,
        HardwareVersion,
        Major,
        Minor,
        Revision,
        Handedness,
        EitherHand,
        LeftHand,
        RightHand,
        BothHands,
        GripPoseOffset = 0x00060040,
        PointerPoseOffset,
    }
    public enum EightAmps : uint
    {
        Switchy = 0xFF8A0001,
        Reddy = 0xFF8A0002,
        SwitchySelector = 0xFF8A0020,
        SwitchyAction = 0xFF8A0030,
        TagId = 0xFF8A0050,
        ReddyProtocolType = 0xFF8A0060,
        ReddyProtocolDataLength = 0xFF8A0061,
        ReddyProtocolData = 0xFF8A0062,
        ReddyStatus = 0xFF8A0063,
        ReddyDecodeTimeout = 0xFF8A0064,
        ReddyEncodeRepeats = 0xFF8A0065,
        ReddyEncodeInterval = 0xFF8A0066,
        ReddyResetCmdReport = 0xFF8A0070,
        ReddyDecodeCmdReport = 0xFF8A0071,
        ReddyEncodeCmdReport = 0xFF8A0072,
        ReddyStatusRspReport = 0xFF8A0073,
        ReddyDecodeRspReport = 0xFF8A0074,
    }
}
