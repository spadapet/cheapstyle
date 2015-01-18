using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComponentAce.Compression.Libs.zlib;

namespace CheapStyle
{
    internal class Image : IDisposable
    {
        private int _width;
        private int _height;
        private ushort[] _colors;
        WriteableBitmap _bitmap;

        private Image()
        {
            _colors = new ushort[0];
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
            ushort[] imageColors = new ushort[width * height];
            byte[] fullData = stream.LoadCompressedBytes();
            Bytes imageStream = new Bytes(fullData);

            for (bool done = false; !done; )
            {
                switch (imageStream.LoadByteAsInt())
                {
                    case 0:
                        done = true;
                        break;

                    case 1:
                        {
                            int y = imageStream.LoadUshortAsInt();
                            int start = imageStream.LoadUshortAsInt();
                            int end = start;
                            int count = imageStream.LoadByteAsInt();

                            while (count != 0)
                            {
                                end = start + count;
                                ushort color = imageStream.LoadColor16();

                                for (int x = start; x < end; x++)
                                {
                                    imageColors[width * y + x] = color;
                                }

                                start = end;
                                count = imageStream.LoadByteAsInt();
                            }
                        }
                        break;

                    case 2:
                        {
                            int y = imageStream.LoadUshortAsInt();
                            int start = imageStream.LoadUshortAsInt();
                            int end = imageStream.LoadUshortAsInt();

                            for (int x = start; x <= end; x++)
                            {
                                ushort color = imageStream.LoadColor16();
                                imageColors[width * y + x] = color;
                            }
                        }
                        break;

                    default:
                        return false;
                }
            }

            _width = width;
            _height = height;
            _colors = imageColors;

            _bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgr565, null);
            _bitmap.Lock();
            _bitmap.WritePixels(new Int32Rect(0, 0, _width, _height), _colors, _width * sizeof(ushort), 0);
            _bitmap.Unlock();
            _bitmap.Freeze();

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
            _width = image._width;
            _height = image._height;
            _colors = image._colors;

            return true;
        }

        public bool Save(string file)
        {
            if (_bitmap != null)
            {
                try
                {
                    using (FileStream tempStream = new FileStream(file, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(_bitmap));
                        encoder.Save(tempStream);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public void Dispose()
        {
        }
    }
}
