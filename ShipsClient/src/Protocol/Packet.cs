using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipsClient.Common
{
    public class Packet : ByteBuffer
    {
        public int Opcode { get; private set; }

        public Packet() : base()
        {
            this.Opcode = 0;
        }

        public Packet(int opcode) : base()
        {
            this.Opcode = opcode;
        }
    }
}
