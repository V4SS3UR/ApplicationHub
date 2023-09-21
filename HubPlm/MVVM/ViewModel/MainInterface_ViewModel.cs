using ApplicationHub.MVVM.Model;
using ApplicationHub.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationHub.MVVM.ViewModel
{
    public class MainInterface_ViewModel : INotifyPropertyChanged
    {
        private AppModel[] originalApplicationList { get; set; }

        private ObservableCollection<AppCategory> _applicationCategoryList; public ObservableCollection<AppCategory> ApplicationCategoryList
        {
            get { return _applicationCategoryList; }
            set { _applicationCategoryList = value; OnPropertyChanged(); }
        }

        private string _searchString; public string SearchString
        {
            private get { return _searchString; }
            set
            {
                _searchString = value;

                AppModel[] filteredAppList = !string.IsNullOrEmpty(value) ? originalApplicationList.Where(o => o.Name.ToLower().Contains(value.ToLower())).ToArray() : originalApplicationList;
                List<IGrouping<string, AppModel>> appGroup = filteredAppList.GroupBy(o => o.Category).ToList();

                AppCategory[] newAppCategory = appGroup.Select(o => new AppCategory(o.Key, o.ToArray())).ToArray();
                ApplicationCategoryList = new ObservableCollection<AppCategory>(newAppCategory);

                OnPropertyChanged();
            }
        }

        public MainInterface_ViewModel()
        {
            originalApplicationList = Constants.GetPaths().Select(o => new AppModel(o)).ToArray();

            SearchString = string.Empty;
        }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}