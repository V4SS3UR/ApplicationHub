using ApplicationHub.MVVM.Model;
using ApplicationHub.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace ApplicationHub.MVVM.ViewModel
{
    public class MainInterface_ViewModel : INotifyPropertyChanged
    {
        private List<AppModel> originalApplicationList { get; set; }

        public ICollectionView ApplicationCategoryListView { get; set; }
        public ObservableCollection<AppCategory> ApplicationCategoryList { get; set; }


        private string _searchString; public string SearchString
        {
            private get { return _searchString; }
            set
            {
                _searchString = value;
                
                foreach (var item in ApplicationCategoryList)
                {
                    item.AppModelListView?.Refresh();
                }
                this.ApplicationCategoryListView.Refresh();

                OnPropertyChanged();
            }
        }

        public MainInterface_ViewModel()
        {
            this.originalApplicationList = new List<AppModel>();
            this.ApplicationCategoryList = new ObservableCollection<AppCategory>();
            this.ApplicationCategoryListView = new CollectionViewSource { Source = this.ApplicationCategoryList }.View;

            Constants.ApplicationPathList.CollectionChanged += ApplicationPathList_CollectionChanged;

            this.SearchString = string.Empty;
        }

        private void ApplicationPathList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (string[] item in e.NewItems)
            {
                AppModel appModel = new AppModel(item);
                this.originalApplicationList.Add(appModel);

                AppCategory category = this.ApplicationCategoryList.FirstOrDefault(o => o.Name == appModel.Category);

                if (category == null)
                {
                    category = new AppCategory(appModel.Category);
                    this.ApplicationCategoryList.Add(category);

                    category.AppModelListView.Filter += o =>
                    {
                        AppModel app = o as AppModel;
                        return string.IsNullOrEmpty(SearchString) || app.Name.ToLower().Contains(SearchString.ToLower());
                    };
                }

                category.AddApp(appModel);
            }
        }



        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}