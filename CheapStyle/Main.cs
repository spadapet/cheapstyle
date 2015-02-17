using System;

namespace CheapStyle
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 2)
            {
                if (!MainWindow.Extract(null, args[0], args[1]))
                {
                    return 1;
                }
            }
            else
            {
                new App().Run();
            }

            return 0;
        }
    }
}
