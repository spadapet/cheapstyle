using System.Collections.Generic;
using System.IO;

namespace CheapStyle
{
    internal class Style
    {
        private uint _posHeader;
        private uint _posStyle;
        private uint _posAuthor;
        private uint _posStandards;
        private uint _posSketches;
        private uint _posErasers;
        private uint _posCharacters;
        private uint _posObjects;
        private uint _posGraphics;
        private uint _posFooter;
        private uint _posMusic;
        private uint _posSounds;
        private uint _posImages;
        private uint _posFiles;

        private Image _imageStandards;
        private Image _imageSketches;
        private Image _imageErasers;
        private Image _imageCharacters;
        private Image _imageObjects;
        private Image _imageGraphics;
        private ICollection<Image> _imageOthers;

        private Style()
        {
            File = string.Empty;
            Header = string.Empty;
            Name = string.Empty;
            Author = string.Empty;

            _imageOthers = new Image[0];
        }

        public static Style Create(string file)
        {
            Style style = new Style();
            return style.Load(file) ? style : null;
        }

        public string File { get; set; }
        public string Header { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }

        public Image GetImage(ImageType type)
        {
            switch (type)
            {
                default:
                    return null;

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
            }
        }

        public ICollection<Image> OtherImages
        {
            get { return _imageOthers; }
        }

        private bool Load(string file)
        {
            try
            {
                File = file;
                return Load(System.IO.File.ReadAllBytes(file));
            }
            catch
            {
                return false;
            }
        }

        private bool Load(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            _posFooter = stream.LoadUint();
            stream.Position = _posFooter;

            _posHeader = stream.LoadUint();
            _posStyle = stream.LoadUint();
            _posAuthor = stream.LoadUint();
            _posStandards = stream.LoadUint();
            _posSketches = stream.LoadUint();
            _posErasers = stream.LoadUint();
            _posCharacters = stream.LoadUint();
            _posObjects = stream.LoadUint();
            _posGraphics = stream.LoadUint();
            _posMusic = stream.LoadUint();
            _posSounds = stream.LoadUint();
            _posImages = stream.LoadUint();
            _posFiles = stream.LoadUint();

            return LoadHeader(bytes) &&
                LoadStandardImages(bytes) &&
                LoadSounds(bytes) &&
                LoadMusic(bytes);
        }

        private bool LoadHeader(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            if (_posHeader != 0)
            {
                stream.Position = _posHeader;
                Header = stream.LoadString();
            }

            if (Header != "Cheapo Copycat Level Editor")
            {
                return false;
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

            return true;
        }

        private bool LoadStandardImages(byte[] bytes)
        {
            _imageStandards = LoadStandardImage(bytes, _posStandards, ImageType.Standards);
            _imageSketches = LoadStandardImage(bytes, _posSketches, ImageType.Sketches);
            _imageErasers = LoadStandardImage(bytes, _posErasers, ImageType.Erasers);
            _imageCharacters = LoadStandardImage(bytes, _posCharacters, ImageType.Characters);
            _imageObjects = LoadStandardImage(bytes, _posObjects, ImageType.Objects);
            _imageGraphics = LoadStandardImage(bytes, _posGraphics, ImageType.Graphics);

            return true;
        }

        private Image LoadStandardImage(byte[] bytes, uint pos, ImageType type)
        {
            Bytes stream = new Bytes(bytes);
            stream.Position = pos;

            return Image.CreateStandard(File, stream, type);
        }

        private bool LoadOtherImages(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);
            stream.Position = _posImages;

            return true;
        }

        private bool LoadSounds(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            return true;
        }

        private bool LoadMusic(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            return true;
        }
    }
}
