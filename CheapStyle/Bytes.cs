using System;
using System.Text;

namespace CheapStyle
{
    internal class Bytes
    {
        public byte[] Buffer { get; private set; }
        public uint Position { get; set; }

        public Bytes(byte[] buffer)
        {
            Buffer = buffer;
        }

        public int IntPos
        {
            get { return (int)Position; }
            set { Position = (uint)value; }
        }

        public int LoadInt()
        {
            int value = BitConverter.ToInt32(Buffer, IntPos);
            Position += sizeof(int);
            return value;
        }

        public uint LoadUint()
        {
            uint value = BitConverter.ToUInt32(Buffer, IntPos);
            Position += sizeof(uint);
            return value;
        }

        public uint LoadUint16()
        {
            ushort value = BitConverter.ToUInt16(Buffer, IntPos);
            Position += sizeof(ushort);
            return value;
        }

        public byte LoadByte()
        {
            return Buffer[Position++];
        }

        public int LoadByteAsInt()
        {
            return (int)LoadByte();
        }

        public uint LoadByteAsUint()
        {
            return (uint)LoadByte();
        }

        public string LoadString()
        {
            int length = LoadByteAsInt();
            string value = Encoding.ASCII.GetString(Buffer, IntPos, length - 1);
            IntPos += length;
            return value;
        }
    }
}
