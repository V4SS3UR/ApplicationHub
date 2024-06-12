using ApplicationHub.MVVM.Model;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationHub.Properties
{
    public class AppFinder
    {
        public event Action<AppModel> OnApplicationFinded;
        public event Action<AppCategory> OnCategoryFinded;

        public AppFinder()
        {
        }

        public async void Find() // Procédure permettant de récupérer les données du fichier DATA
        {
            string assemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
            string initFileLocation = Path.Combine(assemblyLocation, "ApplicationList.txt");
            string category = string.Empty;
            
            string[] lines = File.ReadAllLines(initFileLocation).ToArray();

            //First find all categories and notify
            foreach (string ligne in lines)
            {
                if (ligne.Contains("[") && ligne.Contains("]"))
                {
                    category = ligne.Replace("[", "").Replace("]", "");
                    NotifyCategory(category);
                }
            }

            //Then find apps
            foreach (string ligne in lines)
            {
                if (ligne.Length == 0 || ligne.Contains("//"))
                {
                    continue;
                }

                if (ligne.Contains("[") && ligne.Contains("]"))
                {
                    category = ligne.Replace("[", "").Replace("]", "");
                    continue;
                }

                if (ligne.Contains(".exe"))
                {
                    NotifyAppModel(ligne, category);
                }
                else
                {
                    await Task.Run(() =>
                    {
                        RetrieveFiles(ligne, category);
                    });
                }

            }
        }

        private async Task RetrieveFiles(string path, string category)
        {
            // Retrieve executable files
            FileInfo[] exeFiles = new DirectoryInfo(path).GetFiles("*.exe", SearchOption.TopDirectoryOnly);
            if (exeFiles.Length > 0)
            {
                foreach (FileInfo exeFile in exeFiles)
                {
                    NotifyAppModel(exeFile.FullName, category);                   
                }
            }

            // Retrieve executable shortcuts
            FileInfo[] shortcutFiles = new DirectoryInfo(path).GetFiles("*.lnk", SearchOption.TopDirectoryOnly);
            if (shortcutFiles.Length > 0)
            {
                foreach (FileInfo shortcutFile in shortcutFiles.Where(o => o.Name.Contains(".exe")).ToArray())
                {
                    NotifyAppModel(shortcutFile.FullName, category);
                }
            }
        }

        private void NotifyAppModel(string path, string category)
        {
            var appModel = new AppModel(path, category);
            OnApplicationFinded?.Invoke(appModel);
        }

        private void NotifyCategory(string category)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var appCategory = new AppCategory(category);
                OnCategoryFinded?.Invoke(appCategory);
            });
        }
    }
}