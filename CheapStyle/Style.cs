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
        private uint _posLemmings;
        private uint _posObjects;
        private uint _posGraphics;
        private uint _posFooter;
        private uint _posMusic;
        private uint _posSounds;
        private uint _posImages;
        private uint _posFiles;

        private Style()
        {
            Header = string.Empty;
            Name = string.Empty;
            Author = string.Empty;
        }

        public static Style Create(string file)
        {
            Style style = new Style();
            return style.Load(file) ? style : null;
        }

        public string Header { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }

        private bool Load(string file)
        {
            try
            {
                return Load(File.ReadAllBytes(file));
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
            _posLemmings = stream.LoadUint();
            _posObjects = stream.LoadUint();
            _posGraphics = stream.LoadUint();
            _posMusic = stream.LoadUint();
            _posSounds = stream.LoadUint();
            _posImages = stream.LoadUint();
            _posFiles = stream.LoadUint();

            return LoadHeader(bytes);
        }

        private bool LoadHeader(byte[] bytes)
        {
            Bytes stream = new Bytes(bytes);

            if (_posHeader != 0)
            {
                stream.Position = _posHeader;
                Header = stream.LoadString();
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
    }
}
