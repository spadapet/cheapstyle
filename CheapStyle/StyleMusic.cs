using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CheapStyle
{
    /// <summary>
    /// A MIDI file loaded from a style file
    /// </summary>
    internal class StyleMusic : IDisposable
    {
        private byte[] _data;
        private string _name;

        public StyleMusic(Bytes stream, int index)
        {
            if (stream.LoadByte() != 1)
            {
                throw new Exception("Invalid music format");
            }

            _data = stream.LoadCompressedBytes();
            _name = string.Format("Music{0}", index);
        }

        public void Dispose()
        {
            _data = null;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public void SaveMidi(string filePath)
        {
            System.IO.File.WriteAllBytes(filePath, _data);
        }
    }
}
