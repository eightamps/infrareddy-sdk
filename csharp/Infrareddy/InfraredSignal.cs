using System;
using System.Collections.Generic;
using System.Text;

namespace Infrareddy
{
    public abstract class InfraredSignal
    {
        public enum ProtocolType : byte
        {
            Pronto = 0,
        }

        public abstract byte[] ToRaw();
        public abstract ProtocolType GetProtocol();

        public static InfraredSignal FromRaw(byte[] raw, ProtocolType protocol)
        {
            switch (protocol)
            {
                case ProtocolType.Pronto:
                    return new Pronto(Encoding.ASCII.GetString(raw));
                default:
                    throw new ArgumentOutOfRangeException("protocol", "Outside of known enum range");
            }
        }
    }
}
