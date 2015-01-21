using System.Windows;

namespace CheapStyle
{
    /// <summary>
    /// A sprite loaded from a style file
    /// </summary>
    internal class StyleSprite
    {
        public Int32Rect Rect { get; set; }
        public int HandleX { get; set; }
        public int HandleY { get; set; }
        public bool Metal { get; set; }
        public StyleImage Image { get; private set; }

        public StyleSprite(StyleImage image)
        {
            Image = image;
        }
    }
}
