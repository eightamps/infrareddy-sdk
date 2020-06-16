
namespace EightAmps
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
}
