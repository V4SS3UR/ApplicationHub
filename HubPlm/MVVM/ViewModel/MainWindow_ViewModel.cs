using ApplicationHub.MVVM.View;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ApplicationHub.MVVM.ViewModel
{
    public class MainWindow_ViewModel : INotifyPropertyChanged
    {
        public static MainWindow_ViewModel Instance;

        public static MainInterface_View MainInterface_View { get; set; }

        private string _title; public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _titleVersion; public string TitleVersion
        {
            get { return _titleVersion; }
            set { _titleVersion = value; }
        }

        private object _currentView; public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainWindow_ViewModel()
        {
            Instance = this;

            string assemblyNameVersionMinor = Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            string assemblyNameVersionMajor = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            Title = "Application Hub";
            TitleVersion = $"V{assemblyNameVersionMajor}.{assemblyNameVersionMinor}";

            MainInterface_View = new MainInterface_View();
            CurrentView = MainInterface_View;
        }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}