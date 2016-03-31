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

        public Packet(int opcode, int size = 200) : base()
        {
            this.Opcode = opcode;
        }

        public void Initialize(int opcode, int newres = 200)
        {
            Clear();
            _storage.Capacity = newres;
            Opcode = opcode;
        }
    }
}
