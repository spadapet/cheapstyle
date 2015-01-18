using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CheapStyle
{
    /// <summary>
    /// An image loaded from a style file
    /// </summary>
    internal class StyleImage : IDisposable
    {
        private int _width;
        private int _height;
        private ushort _bgColor;
        private ushort[] _colors;
        private WriteableBitmap _bitmap;

        private StyleImage(Bytes stream, ImageType type)
        {
            LoadStandard(stream, type);
        }

        public void Dispose()
        {
            CopyFrom(null);
        }

        public static StyleImage CreateStandard(Bytes stream, ImageType type)
        {
            return new StyleImage(stream, type);
        }

        private void LoadStandard(Bytes stream, ImageType type)
        {
            switch (stream.LoadByte())
            {
                case 1: // image is here
                    LoadGraphic(stream);
                    LoadSprites(stream);
                    LoadObjects(stream);
                    break;

                case 2: // image is in another style file
                    string name = stream.LoadString();
                    Style style = Style.Create(name);
                    CopyFrom(style.GetImage(type));
                    break;

                default:
                    throw new Exception("Invalid standard image format");
            }
        }

        private void LoadGraphic(Bytes stream)
        {
            if (stream.LoadByte() != 1)
            {
                throw new Exception("Invalid image type");
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
                        throw new Exception("Invalid image row type");
                }
            }

            _width = width;
            _height = height;
            _bgColor = bgColor;
            _colors = imageColors;

            _bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgr565, null);
            _bitmap.Lock();
            _bitmap.WritePixels(new Int32Rect(0, 0, _width, _height), _colors, _width * sizeof(ushort), 0);
            _bitmap.Unlock();
            _bitmap.Freeze();
        }

        private void LoadSprites(Bytes stream)
        {
            // TODO
        }

        private void LoadObjects(Bytes stream)
        {
            // TODO
        }

        private void CopyFrom(StyleImage image)
        {
            _width = 0;
            _height = 0;
            _colors = null;
            _bitmap = null;

            if (image != null)
            {
                _width = image._width;
                _height = image._height;
                _colors = image._colors;
                _bitmap = image._bitmap;
            }
        }

        public void Save(string filePath)
        {
            using (FileStream tempStream = new FileStream(filePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_bitmap));
                encoder.Save(tempStream);
            }
        }
    }
}
