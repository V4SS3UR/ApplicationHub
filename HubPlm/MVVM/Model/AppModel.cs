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
    public class AppModel : ObservableObject
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
        private string _description; public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(); }
        }
        private BitmapSource _icon; public BitmapSource Icon
        {
            get { return _icon; }
            set { _icon = value; OnPropertyChanged(); }
        }
        private BitmapSource _image; public BitmapSource Image
        {
            get { return _image; }
            set { _image = value; OnPropertyChanged(); }
        }

        public string FolderPath => System.IO.Path.GetDirectoryName(Path);
        



        public AppModel(string[] data)
        {
            string filePath = data[0];
            string category = data[1];

            this.Path = filePath;
            this.Name = new FileInfo(filePath).Name.Split(new string[] { ".exe" }, StringSplitOptions.None).First();
            this.Category = category;

            if (File.Exists(filePath))
            {
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                this.Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                icon.Dispose();

                GetMetaData();
            }


            this.OnClickCommand = new RelayCommand(
                o => Process.Start(Path),
                c => File.Exists(Path));
        }

        public void GetMetaData()
        {
            //get path of exe file
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            appPath = System.IO.Path.GetDirectoryName(appPath);
            string metaDataPath = System.IO.Path.Combine(appPath, "metadata", $"{this.Name}");

            if(Directory.Exists(metaDataPath))
            {                
                if(File.Exists(metaDataPath + "\\description.txt"))
                {
                    // Get metadata from the file
                    this.Description = File.ReadAllText(metaDataPath + "\\description.txt");
                    //get image

                    string imagePath = Directory.GetFiles(metaDataPath, "image.*").FirstOrDefault();
                    if(imagePath != null)
                    {
                        this.Image = new BitmapImage(new Uri(imagePath));
                    }
                }
            }
            else
            {
                // Create metadata file
                Directory.CreateDirectory(metaDataPath);
                string metaDataFile = metaDataPath + "\\description.txt";
                File.Create(metaDataFile).Close();
            }
        }
    }
}