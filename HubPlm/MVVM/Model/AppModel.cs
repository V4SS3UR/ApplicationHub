using ApplicationHub.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ApplicationHub.MVVM.Model
{
    public class AppModel : ObservableObject
    {
        public event Action<AppModel> OnClickEvent;
        public event Action<AppModel> OnRemoveEvent;
        public event Action<AppModel, bool> OnPinnedChangeEvent;


        public RelayCommand PinCommand { get; set; }
        public RelayCommand ClickCommand { get; set; }
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand OpenFolderCommand { get; set; }


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

        private bool _isPinned; public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
                _isPinned = value;
                OnPinnedChangeEvent?.Invoke(this, value);
                OnPropertyChanged();
            }
        }
        



        public AppModel(string filePath, string category)
        {
            this.Path = filePath;
            this.Name = new FileInfo(filePath).Name.Split(new string[] { ".exe" }, StringSplitOptions.None).First();
            this.Category = category;

            if (File.Exists(filePath))
            {
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                this.Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.Icon.Freeze();
                icon.Dispose();

                GetMetaData();
            }


            this.ClickCommand = new RelayCommand(
                o => OnClick(),
                c => File.Exists(Path));

            // Open folder of the app
            this.OpenFolderCommand = new RelayCommand(
                o => Process.Start("explorer.exe", FolderPath),
                c => Directory.Exists(FolderPath));

            this.RemoveCommand = new RelayCommand(
                o => OnRemoveEvent?.Invoke(this),
                c => true);
        }

        public void RefreshIcon()
        {
            if (File.Exists(Path))
            {
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(Path);
                this.Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.Icon.Freeze();
                icon.Dispose();
            }
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
                    try
                    {
                        this.Description = File.ReadAllText(metaDataPath + "\\description.txt");
                    }
                    catch (Exception)
                    {
                    }

                    //get image
                    string imagePath = Directory.GetFiles(metaDataPath, "image.*").FirstOrDefault();
                    if(imagePath != null)
                    {
                        this.Image = new BitmapImage(new Uri(imagePath));
                        this.Image.Freeze();
                    }
                }
            }
            else
            {
                // Create metadata file
                Directory.CreateDirectory(metaDataPath);
                string metaDataFile = metaDataPath + "\\description.txt";

                try
                {
                    File.Create(metaDataFile).Close();
                }
                catch (Exception)
                {
                }
            }
        }


        private void OnClick()
        {
            Process.Start(Path);
            OnClickEvent?.Invoke(this);
        }
    }
}