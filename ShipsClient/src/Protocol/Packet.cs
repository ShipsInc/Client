using ShipsClient.Common;

namespace ShipsClient.Protocol
{
    public class Packet : ByteBuffer
    {
        public Opcode Opcode { get; private set; }

        public Packet() : base()
        {
            Opcode = 0;
        }

        public Packet(Opcode opcode) : base()
        {
            Opcode = opcode;
        }
    }
}
