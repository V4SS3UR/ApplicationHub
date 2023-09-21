using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApplicationHub.Properties
{
    public class Constants
    {
        private static List<string[]> applicationPathList;

        private static void Init() // Procédure permettant de récupérer les données du fichier DATA
        {
            string AssemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
            string InitFileLocation = Path.Combine(AssemblyLocation, "ApplicationList.txt");
            string Category = string.Empty;

            applicationPathList = new List<string[]>();
            foreach (string ligne in File.ReadAllLines(InitFileLocation))
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
                    applicationPathList.Add(new string[] { ligne, Category });
                }
                else
                {
                    // Retrieve executable files
                    FileInfo[] exeFiles = new DirectoryInfo(ligne).GetFiles("*.exe", SearchOption.TopDirectoryOnly);
                    if (exeFiles.Length > 0)
                        applicationPathList.AddRange(exeFiles.Select(o => new string[] { o.FullName, Category }));

                    // Retrieve executable shortcuts
                    string[] shortcutFiles = Directory.GetFiles(ligne, "*.lnk");
                    if (shortcutFiles.Length > 0)
                        applicationPathList.AddRange(shortcutFiles.Where(o => o.Contains(".exe")).Select(o => new string[] { o, Category }));
                }
            }
        }

        public static string[][] GetPaths()
        {
            if (applicationPathList == null)
                Init();

            return applicationPathList.ToArray();
        }
    }
}