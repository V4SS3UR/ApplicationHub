﻿using ApplicationHub.Core;
using ApplicationHub.MVVM.View;
using ApplicationHub.Properties;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ApplicationHub.MVVM.ViewModel
{
    public class MainWindow_ViewModel : ObservableObject
    {
        public static MainWindow_ViewModel Instance;

        public static UserControl MainInterface_View { get; set; }

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

        private UserControl _currentView; public UserControl CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        private UserControl _easterView; public UserControl EasterView
        {
            get { return _easterView; }
            set { _easterView = value; OnPropertyChanged(); }
        }

        private bool _simplifiedVersion; public bool SimplifiedVersion
        {
            get { return _simplifiedVersion; }
            set 
            { 
                _simplifiedVersion = value;
                Settings.Default.SimplifiedVersion = value;
                Settings.Default.Save();
                OnSimplifiedVersionChanged();
                OnPropertyChanged(); 
            }
        }

        public MainWindow_ViewModel()
        {
            Instance = this;

            string assemblyNameVersionMinor = Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            string assemblyNameVersionMajor = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            Title = "Application Hub";
            TitleVersion = $"v{assemblyNameVersionMajor}.{assemblyNameVersionMinor}";

            SimplifiedVersion = Settings.Default.SimplifiedVersion;
        }

        private void OnSimplifiedVersionChanged()
        {
            if (SimplifiedVersion)
            {
                MainInterface_View = new SimplifiedListe_View();
            }
            else
            {
                MainInterface_View = new DetailedList_View();
            }

            if(CurrentView != null)
                MainInterface_View.DataContext = CurrentView.DataContext;

            CurrentView = MainInterface_View;
        }
    }
}