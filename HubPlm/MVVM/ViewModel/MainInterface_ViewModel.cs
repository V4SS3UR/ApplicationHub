using ApplicationHub.Core;
using ApplicationHub.Easter.View;
using ApplicationHub.MVVM.Model;
using ApplicationHub.Properties;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace ApplicationHub.MVVM.ViewModel
{
    public class MainInterface_ViewModel : ObservableObject
    {
        public List<AppModel> originalApplicationList { get; set; }
        public ObservableCollection<AppModel> LastUsedApplicationList { get; set; }
        public ListCollectionView LastUsedApplicationListView { get; set; }

        public ObservableCollection<AppModel> CustomApplicationList { get; set; }
        public ListCollectionView CustomApplicationListView { get; set; }

        public ObservableCollection<AppModel> PinnedApplicationList { get; set; }
        public ListCollectionView PinnedApplicationListView { get; set; }



        public ObservableCollection<AppCategory> CategoryList { get; set; }
        public ICollectionView CategoryListView { get; set; }
        private AppCategory _selectedCategory; public AppCategory SelectedCategory
        {
            get { return _selectedCategory; }
            set { 
                _selectedCategory = value; 
                OnPropertyChanged(); 
            }
        }


        private string _searchString; public string SearchString
        {
            private get { return _searchString; }
            set
            {
                _searchString = value;
                
                foreach (var item in CategoryList)
                {
                    item.AppModelListView?.Refresh();
                }
                this.CategoryListView.Refresh();

                CheckEaster();

                OnPropertyChanged();
            }
        }

        public RelayCommand AddAppModelCommand { get; set; }
        public RelayCommand ClearSearchCommand { get; set; }


        public MainInterface_ViewModel()
        {
            this.originalApplicationList = new List<AppModel>();

            this.PinnedApplicationList = new ObservableCollection<AppModel>();
            this.PinnedApplicationListView = new ListCollectionView(this.PinnedApplicationList);

            this.LastUsedApplicationList = new ObservableCollection<AppModel>();
            this.LastUsedApplicationListView = new ListCollectionView(this.LastUsedApplicationList);

            this.CustomApplicationList = new ObservableCollection<AppModel>();
            this.CustomApplicationListView = new ListCollectionView(CustomApplicationList);

            //LastUsedApplicationListView doesn't contain the pinned applications
            this.LastUsedApplicationListView.Filter += o =>
            {
                AppModel app = o as AppModel;
                return !this.PinnedApplicationList.Contains(app);
            };

            this.CategoryList = new ObservableCollection<AppCategory>();
            this.CategoryListView = new CollectionViewSource { Source = this.CategoryList }.View;

            AppFinder appFinder = new AppFinder();
            appFinder.OnApplicationFinded += AppFinder_OnApplicationFinded;
            appFinder.OnCategoryFinded += AppFinder_OnCategoryFinded;

            //Popuplate the originalApplicationList
            Task.Run( () =>
            {
                appFinder.Find();
            })
            .ContinueWith((t) =>
            {
                //Add the path from the settings PersonnalApplicationPathList
                StringCollection personnalApplicationPathList = Settings.Default.PersonnalApplicationPathList;
                if (personnalApplicationPathList != null && personnalApplicationPathList.Count > 0)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (string path in personnalApplicationPathList)
                        {
                            var appModel = new AppModel(path, "Custom");
                            appModel.RefreshIcon();
                            appModel.OnRemoveEvent += RemoveAppModel;
                            appModel.OnPinnedChangeEvent += AppModel_OnPinnedChangeEvent;
                            this.CustomApplicationList.Add(appModel);
                        }
                    });
                }    
            });


            this.SearchString = string.Empty;

            this.CategoryListView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            this.ClearSearchCommand = new RelayCommand(o => this.SearchString = string.Empty);
            this.AddAppModelCommand = new RelayCommand(o => AddAppModel());
        }

        private void AddAppModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var appPath = openFileDialog.FileName;

                FileInfo fileInfo = new FileInfo(appPath);

                AppModel appModel = new AppModel(fileInfo.FullName, "Custom");

                //Add the path to the settings PersonnalApplicationPathList
                StringCollection personnalApplicationPathList = Settings.Default.PersonnalApplicationPathList;
                if(personnalApplicationPathList == null)
                {
                    personnalApplicationPathList = new StringCollection();
                }
                if(!personnalApplicationPathList.Contains(appPath))
                {
                    personnalApplicationPathList.Add(appPath);
                    Settings.Default.PersonnalApplicationPathList = personnalApplicationPathList;
                    Settings.Default.Save();
                }

                this.CustomApplicationList.Clear();
                foreach (string path in personnalApplicationPathList)
                {
                    var app = new AppModel(path, "Custom");
                    app.RefreshIcon();
                    app.OnRemoveEvent += RemoveAppModel;
                    app.OnPinnedChangeEvent += AppModel_OnPinnedChangeEvent;
                    this.CustomApplicationList.Add(app);
                }
            }            
        }
        private void RemoveAppModel(AppModel appModel)
        {
            //Remove the path from the settings PersonnalApplicationPathList
            StringCollection personnalApplicationPathList = Settings.Default.PersonnalApplicationPathList;
            if (personnalApplicationPathList != null && personnalApplicationPathList.Contains(appModel.Path))
            {
                personnalApplicationPathList.Remove(appModel.Path);
                Settings.Default.PersonnalApplicationPathList = personnalApplicationPathList;
                Settings.Default.Save();
            }

            this.CustomApplicationList.Remove(appModel);
        }


        private void AppFinder_OnApplicationFinded(AppModel appModel)
        {
            appModel.RefreshIcon();

            //Add the application to the category   
            App.Current.Dispatcher.Invoke(() =>
            {
                AppCategory category = this.CategoryList.FirstOrDefault(o => o.Name == appModel.Category);
                this.originalApplicationList.Add(appModel);

                if (category != null)
                {
                    if (!category.AppModelList.Contains(appModel))
                    {
                        category.AppModelList.Add(appModel);
                    }
                }
            });
            
            //If there is not selectedApplication, select the first one
            if (this.SelectedCategory == null)
            {
                this.SelectedCategory = this.CategoryList.FirstOrDefault();
            }

            //If the application is pinned, add it to the pinned list
            StringCollection settingsPinnedApplicationList = Settings.Default.PinnedApplicationList;
            if (settingsPinnedApplicationList != null && settingsPinnedApplicationList.Contains(appModel.Name))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //Insert the appModel following it's index in the pinnedApplicationList and the pinnedApplicationList
                    if(PinnedApplicationList.Any(o => o.Name == appModel.Name))
                    {
                        foreach (var pinnedApp in this.PinnedApplicationList)
                        {
                            if (settingsPinnedApplicationList.IndexOf(pinnedApp.Name) > settingsPinnedApplicationList.IndexOf(appModel.Name))
                            {
                                this.PinnedApplicationList.Insert(this.PinnedApplicationList.IndexOf(pinnedApp), appModel);
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.PinnedApplicationList.Add(appModel);
                    }

                    appModel.IsPinned = true;
                });
            }

            //If the application is in the last used list, add it to the last used list
            StringCollection settingsLastUsedApplicationList = Settings.Default.LastUsedApplicationList;
            if (settingsLastUsedApplicationList != null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //Insert the appModel following it's index in the lastUsedApplicationList and the lastUsedApplicationList
                    if (LastUsedApplicationList.Any(o => o.Name == appModel.Name))
                    {
                        foreach (var lastUsedApp in this.LastUsedApplicationList)
                        {
                            if (settingsLastUsedApplicationList.IndexOf(lastUsedApp.Name) > settingsLastUsedApplicationList.IndexOf(appModel.Name))
                            {
                                this.LastUsedApplicationList.Insert(this.LastUsedApplicationList.IndexOf(lastUsedApp), appModel);
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.LastUsedApplicationList.Add(appModel);
                    }
                });
            }
            else
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.LastUsedApplicationList.Add(appModel);
                });
            }

            appModel.OnClickEvent += AppModel_OnClickEvent;
            appModel.OnPinnedChangeEvent += AppModel_OnPinnedChangeEvent;
        }
        private void AppFinder_OnCategoryFinded(AppCategory category)
        {      
            if(this.CategoryList.Any(o => o.Name == category.Name))
            {
                var existingCategory = this.CategoryList.FirstOrDefault(o => o.Name == category.Name);
                category = existingCategory;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                category.AppModelListView.Filter += o =>
                {
                    AppModel app = o as AppModel;
                    return string.IsNullOrEmpty(SearchString) || app.Name.ToLower().Contains(SearchString.ToLower());
                };

                this.CategoryList.Add(category);
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

        private void AppModel_OnClickEvent(AppModel appModel)
        {
            StringCollection lastUsedApplicationList = Settings.Default.LastUsedApplicationList;

            if (lastUsedApplicationList != null)
            {
                if (lastUsedApplicationList.Contains(appModel.Name))
                {
                    lastUsedApplicationList.Remove(appModel.Name);
                    lastUsedApplicationList.Insert(0, appModel.Name);
                }
                else
                {
                    lastUsedApplicationList.Insert(0, appModel.Name);
                }
            }
            else
            {
                lastUsedApplicationList = new StringCollection
                {
                    appModel.Name
                };
            }

            Settings.Default.LastUsedApplicationList = lastUsedApplicationList;
            Settings.Default.Save();

            for(int i = 0; i < lastUsedApplicationList.Count; i++)
            {
                var app = this.LastUsedApplicationList.FirstOrDefault(o => o.Name == lastUsedApplicationList[i]);
                
                if (app != null)
                {
                    this.LastUsedApplicationList.Remove(app);
                    this.LastUsedApplicationList.Insert(i, app);
                }
            } 
        }
        private void AppModel_OnPinnedChangeEvent(AppModel appModel, bool isPinned)
        {
            StringCollection pinnedApplicationList = Settings.Default.PinnedApplicationList;

            if (pinnedApplicationList == null)
            {
                pinnedApplicationList = new StringCollection();
            }

            if (isPinned)
            {
                if (!pinnedApplicationList.Contains(appModel.Name))
                {
                    pinnedApplicationList.Add(appModel.Name);
                }
            }
            else
            {
                if (pinnedApplicationList.Contains(appModel.Name))
                {
                    pinnedApplicationList.Remove(appModel.Name);
                }
            }

            Settings.Default.PinnedApplicationList = pinnedApplicationList;
            Settings.Default.Save();


            App.Current.Dispatcher.Invoke(() =>
            {
                this.PinnedApplicationList.Clear();
                foreach (var appName in pinnedApplicationList)
                {
                    var app = this.originalApplicationList.FirstOrDefault(o => o.Name == appName);
                    if (app == null)
                    {
                        app = this.CustomApplicationList.FirstOrDefault(o => o.Name == appName);
                    }

                    if (app != null)
                    {
                        this.PinnedApplicationList.Add(app);
                    }
                }

                this.LastUsedApplicationListView.Refresh();
                this.PinnedApplicationListView.Refresh();
            });  
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