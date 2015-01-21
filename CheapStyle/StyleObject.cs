using System.Windows;

namespace CheapStyle
{
    internal enum StyleObjectType
    {
        None,
        Window,
        Activate,
        Constant,
    }

    internal enum StyleObjectSubType
    {
        None,
        Exit,
        Water,
        Fire,
        Harmless,
        Death,
        Teleporter,
        Receiver,
        TwoWayTeleporter,
        GravityUp,
        GravityDown,
        Hint,
        SingleTeleporter,
        Splat,
        NoSplat,
    }

    /// <summary>
    /// An object loaded from a style file
    /// </summary>
    internal class StyleObject
    {
        public StyleImage Image { get; private set; }
        public StyleObjectType Type { get; private set; }
        public StyleObjectSubType SubType { get; private set; }
        public int SpriteStart { get; set; }
        public int SpriteCount { get; set; }
        public Int32Rect HitRect { get; set; }
        public int HitPointX { get; set; }
        public int HitPointY { get; set; }
        public int Sound { get; set; }

        public StyleObject(StyleImage image, StyleObjectType type, StyleObjectSubType subType)
        {
            Image = image;
            Type = type;
            SubType = subType;
        }
    }
}
