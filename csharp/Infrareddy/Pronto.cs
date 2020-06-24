using System;
using System.Collections.Generic;
using System.Text;

namespace Infrareddy
{
    public class Pronto : InfraredSignal
    {
        public string String { get; private set; }

        public Pronto(string prontostring)
        {
            this.String = prontostring;
        }

        public override string ToString()
        {
            return this.String;
        }

        public override byte[] ToRaw()
        {
            return Encoding.ASCII.GetBytes(this.String);
        }

        public override ProtocolType GetProtocol()
        {
            return ProtocolType.Pronto;
        }
    }
}
