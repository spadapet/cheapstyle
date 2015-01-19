using System;
using System.Collections.Generic;

namespace CheapStyle
{
    /// <summary>
    /// All images, sounds, etc. loaded from a style file. All styles are
    /// cached until they are disposed.
    /// </summary>
    internal class Style : IDisposable
    {
        private static List<Style> _styleCache;
        private static object _lock;

        private int _posHeader;
        private int _posStyle;
        private int _posAuthor;
        private int _posStandards;
        private int _posSketches;
        private int _posErasers;
        private int _posCharacters;
        private int _posObjects;
        private int _posGraphics;
        private int _posFooter;
        private int _posMusic;
        private int _posSounds;
        private int _posImages;
        private int _posFiles;

        private StyleImage _imageStandards;
        private StyleImage _imageSketches;
        private StyleImage _imageErasers;
        private StyleImage _imageCharacters;
        private StyleImage _imageObjects;
        private StyleImage _imageGraphics;
        private IList<StyleImage> _imageOthers;
        private IList<StyleImage> _allImages;

        static Style()
        {
            _styleCache = new List<Style>();
            _lock = new object();
        }

        private Style(string filePath)
        {
            FilePath = filePath ?? string.Empty;
            Header = string.Empty;
            Name = string.Empty;
            Author = string.Empty;

            _imageOthers = new List<StyleImage>();
            _allImages = new List<StyleImage>();
            _styleCache.Add(this);

            Load(System.IO.File.ReadAllBytes(filePath));
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _styleCache.Remove(this);

                foreach (StyleImage image in AllImages)
                {
                    image.Dispose();
                }
            }
        }

        public static Style Create(string file)
        {
            // Only one style can be loaded at a time
            lock (_lock)
            {
                string name = System.IO.Path.GetFileName(file);

                // See if the style was already loaded
                foreach (Style existingStyle in _styleCache)
                {
                    if (name.Equals(existingStyle.FileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return existingStyle;
                    }
                }

                // See if the file is within the directory of a loaded style
                if (!System.IO.File.Exists(file) && !System.IO.Path.IsPathRooted(file))
                {
                    foreach (Style existingStyle in _styleCache)
                    {
                        string tryPath = System.IO.Path.Combine(existingStyle.FileDir, file);
                        if (System.IO.File.Exists(tryPath))
                        {
                            file = tryPath;
                            break;
                        }
                    }
                }

                return new Style(file);
            }
        }

        public string FilePath { get; set; }
        public string FileName { get { return System.IO.Path.GetFileName(FilePath); } }
        public string FileDir { get { return System.IO.Path.GetDirectoryName(FilePath); } }
        public string Header { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }

        public StyleImage GetImage(StyleImageType type)
        {
            switch (type)
            {
                case StyleImageType.Characters:
                    return _imageCharacters;

                case StyleImageType.Erasers:
                    return _imageErasers;

                case StyleImageType.Graphics:
                    return _imageGraphics;

                case StyleImageType.Objects:
                    return _imageObjects;

                case StyleImageType.Sketches:
                    return _imageSketches;

                case StyleImageType.Standards:
                    return _imageStandards;

                default:
                    return null;
            }
        }

        public ICollection<StyleImage> OtherImages
        {
            get
            {
                return _imageOthers;
            }
        }

        public IEnumerable<StyleImage> AllImages
        {
            get
            {
                return _allImages;
            }
        }

        private void Load(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            _posFooter = stream.LoadInt();
            stream.Position = _posFooter;

            _posHeader = stream.LoadInt();
            _posStyle = stream.LoadInt();
            _posAuthor = stream.LoadInt();
            _posStandards = stream.LoadInt();
            _posSketches = stream.LoadInt();
            _posErasers = stream.LoadInt();
            _posCharacters = stream.LoadInt();
            _posObjects = stream.LoadInt();
            _posGraphics = stream.LoadInt();
            _posFooter = stream.LoadInt();
            _posMusic = stream.LoadInt();
            _posSounds = stream.LoadInt();
            _posImages = stream.LoadInt();
            _posFiles = stream.LoadInt();

            LoadHeader(bytes);
            LoadStandardImages(bytes);
            LoadOtherImages(bytes);
            LoadSounds(bytes);
            LoadMusic(bytes);
        }

        private void LoadHeader(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            if (_posHeader != 0)
            {
                stream.Position = _posHeader;
                Header = stream.LoadString();
            }

            if (Header != "Cheapo Copycat Level Editor")
            {
                throw new Exception("Invalid style file");
            }

            if (_posStyle != 0)
            {
                stream.Position = _posStyle;
                Name = stream.LoadString();
            }

            if (_posAuthor != 0)
            {
                stream.Position = _posAuthor;
                Author = stream.LoadString();
            }
        }

        private void LoadStandardImages(byte[] bytes)
        {
            _imageStandards = LoadStandardImage(bytes, _posStandards, StyleImageType.Standards);
            _imageSketches = LoadStandardImage(bytes, _posSketches, StyleImageType.Sketches);
            _imageErasers = LoadStandardImage(bytes, _posErasers, StyleImageType.Erasers);
            _imageCharacters = LoadStandardImage(bytes, _posCharacters, StyleImageType.Characters);
            _imageObjects = LoadStandardImage(bytes, _posObjects, StyleImageType.Objects);
            _imageGraphics = LoadStandardImage(bytes, _posGraphics, StyleImageType.Graphics);
        }

        private StyleImage LoadStandardImage(byte[] bytes, int pos, StyleImageType type)
        {
            StyleImage image = null;
            if (pos != 0)
            {
                Bytes stream = new Bytes(bytes, pos);
                image = StyleImage.CreateStandard(stream, type);
                _allImages.Add(image);
            }

            return image;
        }

        private void LoadOtherImages(byte[] bytes)
        {
            if (_posImages != 0)
            {
                Bytes stream = new Bytes(bytes, _posImages);
                if (stream.LoadByte() != 1)
                {
                    throw new Exception("Invalid image type in style");
                }

                int count = stream.LoadByteAsInt();
                for (int i = 0; i < count; i++)
                {
                    int index = stream.LoadByteAsInt();
                    StyleImage image = StyleImage.CreateOther(stream, index);
                    _imageOthers.Add(image);
                    _allImages.Add(image);
                }
            }
        }

        private void LoadSounds(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes, _posSounds);

            // TODO
        }

        private void LoadMusic(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes, _posMusic);

            // TODO
        }

        public void Save(string dest)
        {
            foreach (StyleImage image in AllImages)
            {
                string file = System.IO.Path.Combine(dest, image.Name + ".png");
                image.SavePng(file);
            }
        }
    }
}
