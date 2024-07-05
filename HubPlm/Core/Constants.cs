using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ApplicationHub.Properties
{
    public class Constants
    {
        public static ObservableCollection<string[]> ApplicationPathList;
        private static readonly object _lock = new object();


        static Constants()
        {
            ApplicationPathList = new ObservableCollection<string[]>();
            BindingOperations.EnableCollectionSynchronization(ApplicationPathList, _lock);
            Init();
        }

        private static async void Init() // Procédure permettant de récupérer les données du fichier DATA
        {
            string AssemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
            string InitFileLocation = Path.Combine(AssemblyLocation, "ApplicationList.txt");
            string Category = string.Empty;
            
            foreach (string ligne in File.ReadAllLines(InitFileLocation).ToArray())
            {
                
                if (ligne.Length == 0) continue;
                if (ligne.Contains("//")) continue;

                if (ligne.Contains("[") && ligne.Contains("]"))
                {
                    Category = ligne.Replace("[", "").Replace("]", "");
                    continue;
                }

                if (ligne.Contains(".exe"))
                {
                    ApplicationPathList.Add(new string[] { ligne, Category });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        GetFiles(ligne, Category);
                    });
                }

            };
        }

        private static async Task GetFiles(string path, string category)
        {            
            // Retrieve executable files
            FileInfo[] exeFiles = new DirectoryInfo(path).GetFiles("*.exe", SearchOption.TopDirectoryOnly);
            if (exeFiles.Length > 0)
            {
                foreach (FileInfo exeFile in exeFiles)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ApplicationPathList.Add(new string[] { exeFile.FullName, category });
                    });
                }
            }

            // Retrieve executable shortcuts
            FileInfo[] shortcutFiles = new DirectoryInfo(path).GetFiles("*.lnk", SearchOption.TopDirectoryOnly);
            if (shortcutFiles.Length > 0)
            {
                foreach (FileInfo shortcutFile in shortcutFiles.Where(o => o.Name.Contains(".exe")).ToArray())
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ApplicationPathList.Add(new string[] { shortcutFile.FullName, category });
                    });
                }
            }
        }
    }
}