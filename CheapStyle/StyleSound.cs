using System;

namespace CheapStyle
{
    /// <summary>
    /// A WAV file loaded from a style file
    /// </summary>
    internal class StyleSound : IDisposable
    {
        private byte[] _data;
        public string Name { get; private set; }

        public StyleSound(Bytes stream, int index)
        {
            if (stream.LoadByte() != 1)
            {
                throw new Exception("Invalid sound format");
            }

            _data = stream.LoadCompressedBytes();
            Name = string.Format("Sound{0}", index);
        }

        public void Dispose()
        {
            _data = null;
        }

        public void SaveWave(string filePath)
        {
            System.IO.File.WriteAllBytes(filePath, _data);
        }
    }
}
