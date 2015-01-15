using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheapStyle
{
    internal static class StreamExtensions
    {
        public static int LoadInt(this MemoryStream stream)
        {
            byte[] buffer = new byte[4];
            int read = stream.Read(buffer, 0, 4);
            if (read != 4)
            {
                throw new IOException();
            }

            return BitConverter.ToInt32(buffer, 0);
        }

        public static uint LoadUint(this MemoryStream stream)
        {
            byte[] buffer = new byte[4];
            int read = stream.Read(buffer, 0, 4);
            if (read != 4)
            {
                throw new IOException();
            }

            return BitConverter.ToUInt32(buffer, 0);
        }

        public static byte LoadByte(this MemoryStream stream)
        {
            return (byte)stream.ReadByte();
        }

        public static string LoadString(this MemoryStream stream)
        {
            throw new IOException();
        }
    }
}
