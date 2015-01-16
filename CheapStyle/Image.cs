using System.IO;

namespace CheapStyle
{
    internal class Image
    {
        private Image()
        {
        }

        public static Image Create(Bytes stream)
        {
            Image image = new Image();
            return image.Load(stream) ? image : null;
        }

        private bool Load(Bytes stream)
        {
            return true;
        }
    }
}
