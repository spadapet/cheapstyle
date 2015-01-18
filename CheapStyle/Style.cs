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

        public StyleImage GetImage(ImageType type)
        {
            switch (type)
            {
                case ImageType.Characters:
                    return _imageCharacters;

                case ImageType.Erasers:
                    return _imageErasers;

                case ImageType.Graphics:
                    return _imageGraphics;

                case ImageType.Objects:
                    return _imageObjects;

                case ImageType.Sketches:
                    return _imageSketches;

                case ImageType.Standards:
                    return _imageStandards;

                default:
                    return null;
            }
        }

        public ICollection<StyleImage> OtherImages
        {
            get { return _imageOthers; }
        }

        public IEnumerable<StyleImage> AllImages
        {
            get
            {
                yield return _imageCharacters;
                yield return _imageErasers;
                yield return _imageGraphics;
                yield return _imageObjects;
                yield return _imageSketches;
                yield return _imageStandards;

                foreach (StyleImage image in _imageOthers)
                {
                    yield return image;
                }
            }
        }

        private void Load(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            _posFooter = stream.LoadInt();
            stream.IntPos = _posFooter;

            _posHeader = stream.LoadInt();
            _posStyle = stream.LoadInt();
            _posAuthor = stream.LoadInt();
            _posStandards = stream.LoadInt();
            _posSketches = stream.LoadInt();
            _posErasers = stream.LoadInt();
            _posCharacters = stream.LoadInt();
            _posObjects = stream.LoadInt();
            _posGraphics = stream.LoadInt();
            _posMusic = stream.LoadInt();
            _posSounds = stream.LoadInt();
            _posImages = stream.LoadInt();
            _posFiles = stream.LoadInt();

            LoadHeader(bytes);
            LoadStandardImages(bytes);
            LoadSounds(bytes);
            LoadMusic(bytes);
        }

        private void LoadHeader(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            if (_posHeader != 0)
            {
                stream.IntPos = _posHeader;
                Header = stream.LoadString();
            }

            if (Header != "Cheapo Copycat Level Editor")
            {
                throw new Exception("Invalid style file");
            }

            if (_posStyle != 0)
            {
                stream.IntPos = _posStyle;
                Name = stream.LoadString();
            }

            if (_posAuthor != 0)
            {
                stream.IntPos = _posAuthor;
                Author = stream.LoadString();
            }
        }

        private void LoadStandardImages(byte[] bytes)
        {
            _imageStandards = LoadStandardImage(bytes, _posStandards, ImageType.Standards);
            _imageSketches = LoadStandardImage(bytes, _posSketches, ImageType.Sketches);
            _imageErasers = LoadStandardImage(bytes, _posErasers, ImageType.Erasers);
            _imageCharacters = LoadStandardImage(bytes, _posCharacters, ImageType.Characters);
            _imageObjects = LoadStandardImage(bytes, _posObjects, ImageType.Objects);
            _imageGraphics = LoadStandardImage(bytes, _posGraphics, ImageType.Graphics);
        }

        private static StyleImage LoadStandardImage(byte[] bytes, int pos, ImageType type)
        {
            Bytes stream = new Bytes(bytes, pos);
            return StyleImage.CreateStandard(stream, type);
        }

        private void LoadOtherImages(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes, _posImages);

            // TODO
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
    }
}
