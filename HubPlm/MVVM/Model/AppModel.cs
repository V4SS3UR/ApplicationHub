using ApplicationHub.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApplicationHub.Properties
{
    public class AppModel : INotifyPropertyChanged
    {
        private string _path; public string Path
        {
            get { return _path; }
            set { _path = value; OnPropertyChanged(); }
        }

        private string _name; public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _category; public string Category
        {
            get { return _category; }
            set { _category = value; OnPropertyChanged(); }
        }

        private BitmapSource _icon; public BitmapSource Icon
        {
            get { return _icon; }
            set { _icon = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _color; public SolidColorBrush Color
        {
            get { return _color; }
            set { _color = value; OnPropertyChanged(); }
        }

        public RelayCommand Command { get; set; }

        public AppModel(string[] data)
        {
            string filePath = data[0];
            string category = data[1];

            this.Path = filePath;
            this.Name = new FileInfo(filePath).Name.Split(new string[] { ".exe" }, StringSplitOptions.None).First();
            this.Category = category;
            this.Icon = getIcon(filePath);
            this.Color = new SolidColorBrush(LighterColor(GetAverageColor(Icon)));

            this.Command = new RelayCommand(o => Process.Start(Path));
        }

        private BitmapSource getIcon(string filePath)
        {
            var result = (BitmapSource)null;

            try
            {
                using (System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                {
                    result = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch (System.Exception)
            {
                // swallow and return nothing. You could supply a default Icon here as well
            }

            return result;
        }

        public Color LighterColor(Color color)
        {
            System.Drawing.Color lightColor = System.Windows.Forms.ControlPaint.Light(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B), 1f);
            return System.Windows.Media.Color.FromArgb(color.A, lightColor.R, lightColor.G, lightColor.B);
        }

        public System.Windows.Media.Color GetAverageColor(BitmapSource bitmapSource)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;

            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8); // calculate the stride (bytes per row)
            byte[] pixels = new byte[height * stride]; // create a byte array to hold the pixel data

            bitmapSource.CopyPixels(pixels, stride, 0); // copy the pixel data into the byte array

            long totalRed = 0;
            long totalGreen = 0;
            long totalBlue = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + 4 * x; // calculate the index of the current pixel (4 bytes per pixel)

                    byte blue = pixels[index];
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];

                    totalRed += red;
                    totalGreen += green;
                    totalBlue += blue;
                }
            }

            int numPixels = width * height;
            byte averageRed = (byte)(totalRed / numPixels);
            byte averageGreen = (byte)(totalGreen / numPixels);
            byte averageBlue = (byte)(totalBlue / numPixels);

            return System.Windows.Media.Color.FromRgb(averageRed, averageGreen, averageBlue);
        }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}