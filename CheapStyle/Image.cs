using System.IO;
using System.IO.Compression;

namespace CheapStyle
{
    internal class Image
    {
        private Image()
        {
        }

        public static Image CreateStandard(string styleFile, Bytes stream, ImageType type)
        {
            Image image = new Image();
            return image.LoadStandard(styleFile, stream, type) ? image : null;
        }

        private bool LoadStandard(string styleFile, Bytes stream, ImageType type)
        {
            byte loadMethod = stream.LoadByte();

            switch (loadMethod)
            {
                default:
                    return false;

                case 1: // image is here
                    return LoadGraphic(stream) &&
                        LoadSprites(stream) &&
                        LoadObjects(stream);

                case 2: // image is in another style file
                    {
                        string otherStyleName = stream.LoadString();
                        string otherStyleFile = Path.Combine(Path.GetDirectoryName(styleFile), otherStyleName);

                        Style otherStyle = Style.Create(otherStyleFile);
                        return otherStyle != null && CopyFrom(otherStyle.GetImage(type));
                    }
            }
        }

        private bool LoadGraphic(Bytes stream)
        {
            if (stream.LoadByte() != 1)
            {
                return false;
            }

            int width = stream.LoadUshortAsInt();
            int height = stream.LoadUshortAsInt();
            ushort bgColor = stream.LoadColor16();
            int compSize = stream.LoadInt();
            int fullSize = stream.LoadInt();
            byte[] compData = stream.LoadBytes(compSize);
            byte[] fullData = new byte[fullSize];

            // TODO: Uncompress

            return true;
        }

        private bool LoadSprites(Bytes stream)
        {
            return true;
        }

        private bool LoadObjects(Bytes stream)
        {
            return true;
        }

        private bool CopyFrom(Image image)
        {
            return true;
        }
    }
}
