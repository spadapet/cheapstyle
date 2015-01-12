using System.IO;
namespace CheapStyle
{
    internal class Style
    {
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
            return true;
        }
    }
}
