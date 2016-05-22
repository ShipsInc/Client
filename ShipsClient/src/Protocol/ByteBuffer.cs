using System;
using System.IO;
using System.Text;

namespace ShipsClient.Protocol
{
    /// <summary>
    /// Byte buffer.
    /// </summary>
    public class ByteBuffer : IDisposable
    {
        private BinaryWriter w = null;
        private MemoryStream m = null;
        private BinaryReader r = null;

        public ByteBuffer(int size = 1024)
        {
            m = new MemoryStream(size);
            w = new BinaryWriter(m);
            r = new BinaryReader(m);
        }

        public ByteBuffer(byte[] bytes)
        {
            m = new MemoryStream(bytes, true);
            w = new BinaryWriter(m);
            r = new BinaryReader(m, Encoding.UTF8);
        }

        public void Dispose()
        {
            m.Dispose();
            w.Dispose();
        }

        public byte[] ToArray()
        {
            return m.ToArray();
        }

        public byte[] GetBytes(int bytes)
        {
            return r.ReadBytes(bytes);
        }

        public long Length()
        {
            return m.Length;
        }

        public int Capacity()
        {
            return m.Capacity;
        }

        public void ResetPos()
        {
            m.Seek(0, SeekOrigin.Begin);
            w.BaseStream.Seek(0, SeekOrigin.Begin);
            r.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        public void Clear()
        {
            byte[] buffer = m.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            m.Position = 0;
            m.SetLength(0);
        }

        public void WriteInt64(long val)
        {
            w.Write(val);
        }

        public Int64 ReadInt64()
        {
            try
            {
                return r.ReadInt64();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadInt64 empty!");
            }

            return 0;
        }

        public void WriteUInt64(ulong val)
        {
            w.Write(val);
        }

        public UInt64 ReadUInt64()
        {
            try
            {
                return r.ReadUInt64();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadUInt64 empty!");
            }

            return 0;
        }

        public void WriteInt32(int val)
        {
            w.Write(val);
        }

        public Int32 ReadInt32()
        {
            try
            {
                return r.ReadInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadUInt32 empty!");
            }

            return 0;
        }

        public void WriteUInt32(uint val)
        {
            w.Write(val);
        }

        public UInt32 ReadUInt32()
        {
            try
            {
                return r.ReadUInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadUInt32 empty!");
            }

            return 0;
        }

        public void WriteInt16(short val)
        {
            w.Write(val);
        }

        public Int16 ReadInt16()
        {
            try
            {
                return r.ReadInt16();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadInt16 empty!");
            }

            return 0;
        }

        public void WriteUInt16(ushort val)
        {
            w.Write(val);
        }

        public UInt16 ReadUInt16()
        {
            try
            {
                return r.ReadUInt16();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadUInt16 empty!");
            }

            return 0;
        }

        public void WriteUInt8(byte val)
        {
            w.Write(val);
        }

        public UInt16 ReadUInt8()
        {
            try
            {
                return r.ReadByte();
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadByte empty!");
            }

            return 0;
        }

        public void WriteInt8(sbyte val)
        {
            w.Write(val);
        }

        public void WritePackedUInt128(ulong hiPart, ulong loPart)
        {
            var loMask = GetPackedUInt64Mask(loPart);
            var hiMask = GetPackedUInt64Mask(hiPart);

            WritePackedUInt64(loPart, loMask);
            WritePackedUInt64(hiPart, hiMask);
        }

        private byte GetPackedUInt64Mask(ulong val)
        {
            byte mask = 0;
            for (var i = 0; i < 8; ++i)
                if (val >> i * 8 != 0)
                    mask |= (byte)(1 << i);

            return mask;
        }

        public void WritePackedUInt64(ulong val)
        {
            var mask = GetPackedUInt64Mask(val);
            WriteUInt8(mask);
            WritePackedUInt64(val, mask);
        }

        private void WritePackedUInt64(ulong val, byte mask)
        {
            for (var i = 0; i < 8; ++i)
                if ((mask & 1 << i) != 0)
                    WriteUInt8((byte)((val >> i * 8) & 0xFF));
        }

        public void WriteBytes(byte[] bytes)
        {
            w.Write(bytes);
        }

        public void WriteCString(string val, bool nullTerminated = false)
        {
            if (nullTerminated)
                val += '\0';

            var bytes = Encoding.ASCII.GetBytes(val);

            WriteUInt16((ushort)bytes.Length);
            w.Write(bytes);
        }

        public void WriteUTF8String(string val, bool nullTerminated = false)
        {
            if (nullTerminated)
                val += '\0';

            var bytes = Encoding.UTF8.GetBytes(val);
            WriteUInt16((ushort)bytes.Length);
            w.Write(bytes);
        }

        public string ReadUTF8String()
        {
            try
            {
                ushort bytesCount = ReadUInt16();
                byte[] bytes = r.ReadBytes(bytesCount);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadUTF8String empty!");
            }

            return "";
        }

        public string ReadCString()
        {
            try
            {
                ushort bytesCount = ReadUInt16();
                byte[] bytes = r.ReadBytes(bytesCount);
                return Encoding.ASCII.GetString(bytes);
            }
            catch (Exception)
            {
                Console.WriteLine("ByteBuffer: Exeption ReadUTF8String empty!");
            }

            return "";
        }
    }
}
