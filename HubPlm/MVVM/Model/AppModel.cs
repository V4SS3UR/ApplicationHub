using ApplicationHub.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApplicationHub.Properties
{
    public class AppModel : INotifyPropertyChanged
    {
        public RelayCommand OnClickCommand { get; set; }


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




        public AppModel(string[] data)
        {
            string filePath = data[0];
            string category = data[1];

            this.Path = filePath;
            this.Name = new FileInfo(filePath).Name.Split(new string[] { ".exe" }, StringSplitOptions.None).First();
            this.Category = category;            
            this.OnClickCommand = new RelayCommand(o => Process.Start(Path));

            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            this.Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            icon.Dispose();
        }


        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}