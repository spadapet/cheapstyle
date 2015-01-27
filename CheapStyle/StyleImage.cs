using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private ushort[] _pixels;
        private WriteableBitmap _bitmap;
        private StyleImageType _type;
        private string _name;
        private StyleSprite[] _sprites;
        private StyleObject[] _objects;

        private StyleImage(Bytes stream, StyleImageType type)
        {
            _type = type;
            _name = type.ToString();

            Load(stream);
        }

        private StyleImage(Bytes stream, int index)
        {
            _type = StyleImageType.Other;
            _name = string.Format("Other{0}", index);

            Load(stream);
        }

        public void Dispose()
        {
            CopyFrom(null);
        }

        public static StyleImage CreateStandard(Bytes stream, StyleImageType type)
        {
            return new StyleImage(stream, type);
        }

        public static StyleImage CreateOther(Bytes stream, int index)
        {
            return new StyleImage(stream, index);
        }

        private void Load(Bytes stream)
        {
            switch (_type != StyleImageType.Other ? stream.LoadByte() : 1)
            {
                case 1: // image is here
                    LoadGraphic(stream);
                    if (_type != StyleImageType.Other)
                    {
                        LoadSprites(stream);
                        LoadObjects(stream);
                    }
                    break;

                case 2: // image is in another style file
                    string name = stream.LoadString();
                    Style style = Style.Create(name);
                    CopyFrom(style.GetImage(_type));
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
            ushort[] pixels = new ushort[width * height];
            byte[] imageBytes = stream.LoadCompressedBytes();
            Bytes imageStream = new Bytes(imageBytes);

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bgColor;
            }

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
                                    pixels[width * y + x] = color;
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
                                pixels[width * y + x] = color;
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
            _pixels = pixels;

            _bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgr565, null);
            _bitmap.Lock();
            _bitmap.WritePixels(new Int32Rect(0, 0, _width, _height), _pixels, _width * sizeof(ushort), 0);
            _bitmap.Unlock();
            _bitmap.Freeze();
        }

        private void LoadSprites(Bytes stream)
        {
            if (stream.LoadByte() != 1)
            {
                throw new Exception("Invalid sprite format");
            }

            int count = stream.LoadUshortAsInt();
            _sprites = new StyleSprite[count];

            for (int i = 0; i < count; i++)
            {
                int cur = stream.LoadUshortAsInt();
                if (cur < 0 || cur >= count)
                {
                    throw new Exception("Invalid sprite index");
                }

                int left = stream.LoadUshortAsInt();
                int top = stream.LoadUshortAsInt();
                int right = stream.LoadUshortAsInt();
                int bottom = stream.LoadUshortAsInt();

                _sprites[cur] = new StyleSprite(this);
                _sprites[cur].Rect = new Int32Rect(left, top, right - left + 1, bottom - top + 1);
            }
        }

        private void LoadObjects(Bytes stream)
        {
            if (stream.LoadByte() != 1)
            {
                throw new Exception("Invalid object format");
            }

            int count = stream.LoadUshortAsInt();
            int objectCount = stream.LoadUshortAsInt();

            _objects = new StyleObject[objectCount];

            for (int i = 0, curObj = 0; i < count; i++)
            {
                int type = stream.LoadByteAsInt();
                switch (type)
                {
                    case 1: // handle
                        {
                            int start = stream.LoadUshortAsInt();
                            int end = stream.LoadUshortAsInt();
                            int x = stream.LoadShortAsInt();
                            int y = stream.LoadShortAsInt();

                            for (int h = start; h <= end; h++)
                            {
                                _sprites[h].HandleX = x;
                                _sprites[h].HandleY = y;
                            }
                        }
                        break;

                    case 20: // metal
                        {
                            int start = stream.LoadUshortAsInt();
                            int end = stream.LoadUshortAsInt();

                            for (int h = start; h <= end; h++)
                            {
                                _sprites[h].Metal = true;
                            }
                        }
                        break;

                    case 2: // window
                        {
                            int start = stream.LoadUshortAsInt();
                            int end = stream.LoadUshortAsInt();
                            int x = stream.LoadShortAsInt();
                            int y = stream.LoadShortAsInt();

                            _objects[curObj] = new StyleObject(this, StyleObjectType.Window, StyleObjectSubType.None);
                            _objects[curObj].SpriteStart = start;
                            _objects[curObj].SpriteCount = end - start + 1;
                            _objects[curObj].HitPointX = x;
                            _objects[curObj].HitPointY = y;
                            curObj++;
                        }
                        break;

                    case 3: // activation object
                    case 4: // constant object
                        {
                            StyleObjectType objType = (type == 3) ? StyleObjectType.Activate : StyleObjectType.Constant;
                            StyleObjectSubType subType = (StyleObjectSubType)stream.LoadByteAsInt();
                            int start = stream.LoadUshortAsInt();
                            int end = stream.LoadUshortAsInt();
                            int x1 = stream.LoadShortAsInt();
                            int y1 = stream.LoadShortAsInt();
                            int x2 = stream.LoadShortAsInt();
                            int y2 = stream.LoadShortAsInt();
                            int ptx = stream.LoadShortAsInt();
                            int pty = stream.LoadShortAsInt();
                            int sound = stream.LoadByteAsInt();

                            _objects[curObj] = new StyleObject(this, objType, subType);
                            _objects[curObj].SpriteStart = start;
                            _objects[curObj].SpriteCount = end - start + 1;
                            _objects[curObj].HitRect = new Int32Rect(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
                            _objects[curObj].HitPointX = ptx;
                            _objects[curObj].HitPointY = pty;
                            _objects[curObj].Sound = sound;
                            curObj++;
                        }
                        break;

                    default:
                        throw new Exception("Invalid object type");
                }
            }
        }

        private void CopyFrom(StyleImage image)
        {
            _width = 0;
            _height = 0;
            _pixels = null;
            _bitmap = null;
            _sprites = null;
            _objects = null;

            if (image != null)
            {
                _width = image._width;
                _height = image._height;
                _pixels = image._pixels;
                _bitmap = image._bitmap;
                _sprites = image._sprites;
                _objects = image._objects;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public IEnumerable<StyleSprite> Sprites
        {
            get
            {
                return _sprites;
            }
        }

        public IEnumerable<StyleObject> Objects
        {
            get
            {
                return _objects;
            }
        }

        public void SavePng(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_bitmap));
                encoder.Save(stream);
            }
        }

        public void SaveSprites(string filePath)
        {
            if (_sprites != null && _sprites.Length > 0)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                using (TextWriter writer = new StreamWriter(stream, Encoding.ASCII))
                {
                    writer.WriteLine("<?xml version='1.0' ?>");
                    writer.WriteLine("<Sprites>");

                    int i = 0;
                    foreach (StyleSprite sprite in _sprites)
                    {
                        writer.WriteLine("    <Sprite Index='{0}' StartX='{4}' StartY='{5}' Width='{6}' Height='{7}' HandleX='{2}' HandleY='{3}' Metal='{1}' />",
                            i++,
                            sprite.Metal,
                            sprite.HandleX,
                            sprite.HandleY,
                            sprite.Rect.X,
                            sprite.Rect.Y,
                            sprite.Rect.Width,
                            sprite.Rect.Height);
                    }

                    writer.WriteLine("</Sprites>");
                }
            }
        }

        public void SaveObjects(string filePath)
        {
            if (_objects != null && _objects.Length > 0)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                using (TextWriter writer = new StreamWriter(stream, Encoding.ASCII))
                {
                    writer.WriteLine("<?xml version='1.0' ?>");
                    writer.WriteLine("<Objects>");

                    int i = 0;
                    foreach (StyleObject obj in _objects)
                    {
                        writer.WriteLine("    <Object Index='{0}' Type='{1}' SubType='{2}' SpriteStart='{3}' SpriteCount='{4}' HitRectX='{5}' HitRectY='{6}' HitRectWidth='{7}' HitRectHeight='{8}' HitPointX='{9}' HitPointY='{10}' Sound='{11}' />",
                            i++,
                            obj.Type,
                            obj.SubType,
                            obj.SpriteStart,
                            obj.SpriteCount,
                            obj.HitRect.X,
                            obj.HitRect.Y,
                            obj.HitRect.Width,
                            obj.HitRect.Height,
                            obj.HitPointX,
                            obj.HitPointY,
                            obj.Sound);
                    }

                    writer.WriteLine("</Objects>");
                }
            }
        }
    }
}
