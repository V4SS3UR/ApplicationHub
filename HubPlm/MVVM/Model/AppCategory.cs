using ApplicationHub.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ApplicationHub.MVVM.Model
{
    public class AppCategory : INotifyPropertyChanged
    {
        private string _name; public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AppModel> _appModelList; public ObservableCollection<AppModel> AppModelList
        {
            get { return _appModelList; }
            set { _appModelList = value; OnPropertyChanged(); }
        }

        public AppCategory(string name, AppModel[] appModelList)
        {
            this.Name = name;

            appModelList = appModelList.OrderBy(o => o.Name).ToArray();
            this.AppModelList = new ObservableCollection<AppModel>(appModelList);
        }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}