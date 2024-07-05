using ApplicationHub.Core;
using ApplicationHub.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace ApplicationHub.MVVM.Model
{
    public class AppCategory : ObservableObject
    {
        private string _name; public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public ICollectionView AppModelListView { get; set; }
        private ObservableCollection<AppModel> _appModelList; public ObservableCollection<AppModel> AppModelList
        {
            get { return _appModelList; }
            set { _appModelList = value; OnPropertyChanged(); }
        }
        private AppModel _selectedAppModel; public AppModel SelectedAppModel
        {
            get { return _selectedAppModel; }
            set { _selectedAppModel = value; OnPropertyChanged(); }
        }


        public AppCategory(string name)
        {
            this.Name = name;

            this.AppModelList = new ObservableCollection<AppModel>();
            this.AppModelListView = new CollectionViewSource { Source = this.AppModelList }.View;

            this.AppModelListView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            this.SelectedAppModel = this.AppModelList.FirstOrDefault();
        }


        public void AddApp(AppModel appModel)
        {
            App.Current.Dispatcher.Invoke((Action)(delegate
            {
                this.AppModelList.Add(appModel);
            }));
        }
    }
}