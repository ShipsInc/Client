using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShipsClient.Common
{
    /// <summary>
    /// Byte buffer.
    /// </summary>
    public class ByteBuffer : IDisposable
    {
        #region Constructor and member variables
        protected List<byte> _storage;
        private int ReadHead = 0;

        public int Count
        {
            get { return _storage.Count; }
        }

        public int Length
        {
            get { return Count - ReadHead; }
        }

        /// <summary>
        /// Creates a new ByteBuffer instance.
        /// </summary>
        public ByteBuffer()
        {
            this._storage = new List<byte>();
        }

        public ByteBuffer(int size)
        {
            this._storage = new List<byte>(size);
        }

        /// <summary>
        /// Creates a new ByteBuffer instance.
        /// </summary>
        /// <param name='Data'> Data for pre-allocation. </param>
        public ByteBuffer(byte[] data)
        {
            this._storage = new List<byte>(data);
        }
        #endregion

        /// <summary> 
        /// Check if everything has been read, and if so, flush it.
        /// </summary>
        private bool Trim(int size)
        {
            // If the readhead is past the buffersize, this means everything
            // has been read in the array, and the user is trying read something inexistent,
            // then throw a exception.
            if (this.Length - size < 0)
            {
                throw new BufferOverflowException("Byte buffer limit exceeded.");
            }

            // If we're here, this means everthing is ok
            // then just return true;
            return true;
        }

        /// <summary>
        /// Copies the elements in a list to a new array.
        /// </summary>
        public byte[] ToArray()
        {
            return _storage.ToArray();
        }

        /// <summary>
        /// Moves the read head.
        /// </summary>
        /// <param name='head'> Head. </param>
        public void MoveReadHead(int head)
        {
            Trim(head);
            this.ReadHead += head;
        }

        /// <summary>
        /// Resets the read head.
        /// </summary>
        public void Reset()
        {
            this.ReadHead = 0;
        }

        /// <summary>
        /// Clear this instance.
        /// </summary>
        public void Clear()
        {
            this.ReadHead = 0;
            this._storage.Clear();
        }
        /// <summary>
        /// Store a string on byte buffer.
        /// </summary>
        /// 
        /// <param name="input">String.</param>
        public void WriteString(string input)
        {
            WriteInt32(input.Length);
            WriteBytes(Encoding.ASCII.GetBytes(input));
        }

        /// <summary>
        /// Store a single byte on byte buffer.
        /// </summary>
        /// 
        /// <param name="input">A byte.</param>
        public void WriteByte(byte input)
        {
            _storage.Add(input);
        }

        /// <summary>
        /// Store a single byte on byte buffer.
        /// </summary>
        /// <param name='input'> The input data. It will be cast as a byte. </param>
        public void WriteByte(object input)
        {
            _storage.Add((byte)input);
        }

        /// <summary>
        /// Store a sequence of bytes on byte buffer.
        /// </summary>
        /// <param name="input">The byte array.</param>
        public void WriteBytes(byte[] input)
        {
            _storage.AddRange(input);
        }

        /// <summary>
        /// Store a short integer on byte buffer.
        /// </summary>
        /// 
        /// <param name="input">Short.</param>
        public void WriteInt16(short input)
        {
            WriteBytes(BitConverter.GetBytes(input));
        }

        /// <summary>
        /// Store a integer on byte buffer.
        /// </summary>
        /// <param name="input">Integer.</param>
        public void WriteInt32(int input)
        {
            WriteBytes(BitConverter.GetBytes(input));
        }

        /// <summary>
        /// Store a long integer on byte buffer.
        /// </summary>
        /// 
        /// <param name="input">Long.</param>
        public void WriteInt64(long input)
        {
            WriteBytes(BitConverter.GetBytes(input));
        }

        /// <summary>
        /// Retrieve a string from the byte buffer.
        /// </summary>
        /// 
        /// <param name="peek">Move Read Head</param>
        public string ReadString(bool peek)
        {
            int length = ReadInt32(peek);
            int Start = ReadHead;

            if (length <= 0)
            {
                return String.Empty;
            }

            if (peek == false)
            {
                Start += 4;
            }

            return Encoding.ASCII.GetString(
                ReadBytes(Start, length, peek)
            );
        }

        /// <summary>
        /// Retrieve a single byte from the byte buffer.
        /// </summary>
        /// 
        /// <param name="peek">Move Read Head</param>
        public byte ReadByte(bool peek)
        {
            Trim(1);

            if (peek)
            {
                return _storage[ReadHead++];
            }

            return _storage[ReadHead];
        }

        /// <summary>
        /// Retrieve a sequence of bytes from byte buffer.
        /// </summary>
        /// 
        /// <param name="length">length of the byte array</param>
        /// <param name="peek">Move Read Head</param>
        public byte[] ReadBytes(int length, bool peek)
        {
            return ReadBytes(ReadHead, length, peek);
        }

        /// <summary>
        /// Retrieve a sequence of bytes from byte buffer.
        /// </summary>
        /// 
        /// <param name="start">Where start to read.</param>
        /// <param name="length">length of the byte array</param>
        /// <param name="peek">Move Read Head</param>
        private byte[] ReadBytes(int start, int length, bool peek)
        {
            Trim(length);

            if (length <= 0)
            {
                return null;
            }

            if (peek)
            {
                ReadHead += length;
            }

            return _storage.GetRange(start, length).ToArray();
        }

        /// <summary>
        /// Retrieve a short integer from the byte buffer.
        /// </summary>
        /// 
        /// <param name="peek">Move Read Head</param>
        public short ReadInt16(bool peek)
        {
            return BitConverter.ToInt16(ReadBytes(2, peek), 0);
        }

        /// <summary>
        /// Retrieve a integer from the byte buffer.
        /// </summary>
        /// 
        /// <param name="peek">Move Read Head</param>
        public int ReadInt32(bool peek)
        {
            return BitConverter.ToInt32(ReadBytes(4, peek), 0);
        }

        /// <summary>
        /// Retrieve a long integer from the byte buffer.
        /// </summary>
        /// 
        /// <param name="peek">Move Read Head</param>
        public long ReadInt64(bool peek)
        {
            return BitConverter.ToInt64(ReadBytes(8, peek), 0);
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.

            _storage = null;
            ReadHead = 0;
        }
        #endregion
    }

    /// <summary>
    /// Buffer overflow exception.
    /// </summary>
    internal class BufferOverflowException : OverflowException, ISerializable
    {
        public BufferOverflowException() : base()
        {
            // Add implementation.
        }

        public BufferOverflowException(string message) : base(message)
        {
            // Add implementation.
        }

        public BufferOverflowException(string message, Exception inner) : base(message, inner)
        {
            // Add implementation.
        }

        // This constructor is needed for serialization.
        protected BufferOverflowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Add implementation.
        }
    }
}
