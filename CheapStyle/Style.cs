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
        }

        public static Style Create(string file)
        {
            Style style = new Style();
            if (style.Load(file))
            {
                return style;
            }

            return null;
        }

        private bool Load(string file)
        {
            byte[] bytes = null;

            try
            {
                bytes = File.ReadAllBytes(file);
            }
            catch
            {
                return false;
            }

            MemoryStream stream = new MemoryStream(bytes);

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

            return true;
        }
    }
}
