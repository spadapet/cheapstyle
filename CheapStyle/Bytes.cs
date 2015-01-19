using System;
using System.IO;
using System.Text;
using ComponentAce.Compression.Libs.zlib;

namespace CheapStyle
{
    internal class Bytes
    {
        public byte[] Buffer { get; private set; }
        public int Position { get; set; }

        public Bytes(byte[] buffer, int position = 0)
        {
            Buffer = buffer;
            Position = position;
        }

        public int LoadInt()
        {
            int value = BitConverter.ToInt32(Buffer, Position);
            Position += sizeof(int);
            return value;
        }

        public uint LoadUint()
        {
            uint value = BitConverter.ToUInt32(Buffer, Position);
            Position += sizeof(uint);
            return value;
        }

        public ushort LoadUshort()
        {
            ushort value = BitConverter.ToUInt16(Buffer, Position);
            Position += sizeof(ushort);
            return value;
        }

        public int LoadUshortAsInt()
        {
            ushort value = BitConverter.ToUInt16(Buffer, Position);
            Position += sizeof(ushort);
            return (int)value;
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
            string value = Encoding.ASCII.GetString(Buffer, Position, length - 1);
            Position += length;
            return value;
        }

        public ushort LoadColor16()
        {
            return LoadUshort();
        }

        public byte[] LoadBytes(int size)
        {
            byte[] bytes = new byte[size];
            Array.Copy(Buffer, Position, bytes, 0, size);
            Position += size;
            return bytes;
        }

        public byte[] LoadCompressedBytes()
        {
            int compSize = LoadInt();
            int fullSize = LoadInt();
            byte[] compData = LoadBytes(compSize);
            byte[] fullData = new byte[fullSize];

            MemoryStream fullDataStream = new MemoryStream(fullData, true);
            ZOutputStream zlibStream = new ZOutputStream(fullDataStream);
            zlibStream.Write(compData, 0, compSize);

            return fullData;
        }
    }
}
