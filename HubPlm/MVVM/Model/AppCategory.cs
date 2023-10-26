using ApplicationHub.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace ApplicationHub.MVVM.Model
{
    public class AppCategory : INotifyPropertyChanged
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

        public AppCategory(string name)
        {
            this.Name = name;

            this.AppModelList = new ObservableCollection<AppModel>();
            this.AppModelListView = new CollectionViewSource { Source = this.AppModelList }.View;            
        }


        public void AddApp(AppModel appModel)
        {
            App.Current.Dispatcher.Invoke((Action)(delegate
            {
                this.AppModelList.Add(appModel);
            }));
        }




        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}