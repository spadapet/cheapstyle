using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CheapStyle
{
    public partial class MainWindow : Window, System.Windows.Forms.IWin32Window
    {
        private bool _manuallyChoseDest;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseStyle()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.DefaultExt = ".sty";
            dialog.FileName = StyleTextBox.Text;
            dialog.Filter = "Style Files|*.sty|All Files|*.*";
            dialog.Title = "Choose a style file";

            bool? result = dialog.ShowDialog(this);
            if (result.HasValue && result.Value)
            {
                StyleTextBox.Text = dialog.FileName;

                if (!_manuallyChoseDest)
                {
                    string dir = Path.GetDirectoryName(dialog.FileName);
                    string file = Path.GetFileNameWithoutExtension(dialog.FileName);

                    DestTextBox.Text = Path.Combine(dir, file);
                }
            }
        }

        private void ChooseDest()
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = DestTextBox.Text;
                dialog.Description = "Choose a destination for extracted files";
                if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    DestTextBox.Text = dialog.SelectedPath;
                    _manuallyChoseDest = true;
                }
            }
        }

        private void OnChooseStyle(object sender, RoutedEventArgs args)
        {
            ChooseStyle();
        }

        private void OnChooseDest(object sender, RoutedEventArgs args)
        {
            ChooseDest();
        }

        private void OnOk(object sender, RoutedEventArgs args)
        {
            string file = StyleTextBox.Text;
            string dest = DestTextBox.Text;

            Extract(this, file, dest);
        }

        private void OnCancel(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (!args.Handled && args.Key == Key.Escape)
            {
                args.Handled = true;
                Close();
            }
        }

        IntPtr System.Windows.Forms.IWin32Window.Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        public static bool Extract(MainWindow window, string file, string dest)
        {
            try
            {
                if (!File.Exists(file))
                {
                    string message = "File doesn't exist: " + file;

                    if (window != null)
                    {
                        MessageBox.Show(window, message);
                    }
                    else
                    {
                        Console.Error.WriteLine(message);
                    }

                    return false;
                }

                if (!Directory.Exists(dest))
                {
                    try
                    {
                        DirectoryInfo info = Directory.CreateDirectory(dest);
                        if (info == null)
                        {
                            throw new IOException();
                        }
                    }
                    catch
                    {
                        string message = "Directory can't be created: " + dest;

                        if (window != null)
                        {
                            MessageBox.Show(window, message);
                        }
                        else
                        {
                            Console.Error.WriteLine(message);
                        }

                        return false;
                    }
                }

                using (CheapStyle.Style style = CheapStyle.Style.Create(file))
                {
                    style.Save(dest);
                }
            }
            catch (Exception exception)
            {
                string message = string.Format("Invalid style file: {0}\r\nError message: {1}", file, exception.Message);

                if (window != null)
                {
                    MessageBox.Show(window, message);
                }
                else
                {
                    Console.Error.WriteLine(message);
                }

                return false;
            }

            if (window != null)
            {
                Process.Start(dest);
                window.Close();
            }

            return true;
        }
    }
}
