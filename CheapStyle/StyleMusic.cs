﻿using System;

namespace CheapStyle
{
    /// <summary>
    /// A MIDI file loaded from a style file
    /// </summary>
    internal class StyleMusic : IDisposable
    {
        private byte[] _data;
        public string Name { get; private set; }

        public StyleMusic(Bytes stream, int index)
        {
            if (stream.LoadByte() != 1)
            {
                throw new Exception("Invalid music format");
            }

            _data = stream.LoadCompressedBytes();
            Name = string.Format("Music{0}", index);
        }

        public void Dispose()
        {
            _data = null;
        }

        public void SaveMidi(string filePath)
        {
            System.IO.File.WriteAllBytes(filePath, _data);
        }
    }
}
