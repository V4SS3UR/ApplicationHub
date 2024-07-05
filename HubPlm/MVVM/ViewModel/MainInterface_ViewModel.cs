using ApplicationHub.Core;
using ApplicationHub.Easter.View;
using ApplicationHub.MVVM.Model;
using ApplicationHub.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ApplicationHub.MVVM.ViewModel
{
    public class MainInterface_ViewModel : ObservableObject
    {
        private List<AppModel> originalApplicationList { get; set; }

        public ICollectionView ApplicationCategoryListView { get; set; }
        public ObservableCollection<AppCategory> ApplicationCategoryList { get; set; }
        private AppCategory _selectedApplicationCategory; public AppCategory SelectedApplicationCategory
        {
            get { return _selectedApplicationCategory; }
            set { 
                _selectedApplicationCategory = value; 
                OnPropertyChanged(); 
            }
        }


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

                CheckEaster();

                OnPropertyChanged();
            }
        }

        public RelayCommand ClearSearchCommand { get; set; }


        public MainInterface_ViewModel()
        {
            this.originalApplicationList = new List<AppModel>();
            this.ApplicationCategoryList = new ObservableCollection<AppCategory>();
            this.ApplicationCategoryListView = new CollectionViewSource { Source = this.ApplicationCategoryList }.View;

            AppFinder appFinder = new AppFinder();
            appFinder.OnApplicationFinded += AppFinder_OnApplicationFinded;
            appFinder.OnCategoryFinded += AppFinder_OnCategoryFinded;
            Task.Run(() => appFinder.Find());

            this.SearchString = string.Empty;

            this.ApplicationCategoryListView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            this.ClearSearchCommand = new RelayCommand(o => this.SearchString = string.Empty);
        }

        

        private void AppFinder_OnApplicationFinded(AppModel appModel)
        {
            AppCategory category = this.ApplicationCategoryList.FirstOrDefault(o => o.Name == appModel.Category);

            App.Current.Dispatcher.Invoke(() =>
            {
                this.originalApplicationList.Add(appModel);

                if (category != null)
                {
                    if (!category.AppModelList.Contains(appModel))
                    {
                        category.AppModelList.Add(appModel);
                    }
                }
            });

            

            if (this.SelectedApplicationCategory == null)
            {
                this.SelectedApplicationCategory = this.ApplicationCategoryList.FirstOrDefault();
            }
        }

        private void AppFinder_OnCategoryFinded(AppCategory category)
        {      
            App.Current.Dispatcher.Invoke(() =>
            {
                category.AppModelListView.Filter += o =>
                {
                    AppModel app = o as AppModel;
                    return string.IsNullOrEmpty(SearchString) || app.Name.ToLower().Contains(SearchString.ToLower());
                };
                this.ApplicationCategoryList.Add(category);
            });

            // Add original applications to the category
            var apps = this.originalApplicationList.Where(o => o.Category == category.Name).ToList();
            foreach (var app in apps)
            {
                if (!category.AppModelList.Contains(app))
                {
                    category.AppModelList.Add(app);                
                }
            }
        }


        private void CheckEaster()
        {
            if(this.SearchString == "FLAPPY")
            {
                var flappy = new Flappy_View();
                flappy.Flappy.CloseGameAction = new Action(() => MainWindow_ViewModel.Instance.EasterView = null);
                MainWindow_ViewModel.Instance.EasterView = flappy;
                this.SearchString = string.Empty;
            }
        }
    }
}