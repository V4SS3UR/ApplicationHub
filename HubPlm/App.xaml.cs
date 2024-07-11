using ApplicationHub.Properties;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;

namespace ApplicationHub
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Load the settings from local storage
            var localDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            localDirectory = Path.Combine(localDirectory, "ApplicationHub");

            if (!Directory.Exists(localDirectory))
            {
                return;
            }

            string savedPinnedApplicationList = System.IO.Path.Combine(localDirectory, "PinnedApplicationList.txt");
            string savedPersonnalApplicationPathList = System.IO.Path.Combine(localDirectory, "PersonnalApplicationPathList.txt");

            //If the file exists, and its content is different from the currentValue, we update the currentValue
            if (File.Exists(savedPinnedApplicationList))
            {
                var settingsPinnedApplicationList = File.ReadAllLines(savedPinnedApplicationList);
                if (settingsPinnedApplicationList != null)
                {
                    var currentPinnedApplicationList = Settings.Default.PinnedApplicationList;
                    
                    if(currentPinnedApplicationList == null || !settingsPinnedApplicationList.SequenceEqual(currentPinnedApplicationList.Cast<string>()))
                    {
                        StringCollection strings = new StringCollection();
                        strings.AddRange(settingsPinnedApplicationList);
                        Settings.Default.PinnedApplicationList = strings;
                    }
                }
            }

            if (File.Exists(savedPersonnalApplicationPathList))
            {
                var settingsPersonnalApplicationPathList = File.ReadAllLines(savedPersonnalApplicationPathList);
                if (settingsPersonnalApplicationPathList != null)
                {
                    var currentPersonnalApplicationPathList = Settings.Default.PersonnalApplicationPathList;

                    if (currentPersonnalApplicationPathList == null || !settingsPersonnalApplicationPathList.SequenceEqual(currentPersonnalApplicationPathList.Cast<string>()))
                    {
                        StringCollection strings = new StringCollection();
                        strings.AddRange(settingsPersonnalApplicationPathList);
                        Settings.Default.PersonnalApplicationPathList = strings;
                    }
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            //Save the settings in local storage
            var localDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            localDirectory = Path.Combine(localDirectory, "ApplicationHub");

            if(!Directory.Exists(localDirectory))
            {
                Directory.CreateDirectory(localDirectory);
            }

            var pinnedApplicationSettings = Settings.Default.PinnedApplicationList;
            var PersonnalApplicationPathList = Settings.Default.PersonnalApplicationPathList;

            if (pinnedApplicationSettings != null)
            {
                string pinnedApplicationSettingsPath = System.IO.Path.Combine(localDirectory, "PinnedApplicationList.txt");
                System.IO.File.WriteAllLines(pinnedApplicationSettingsPath, pinnedApplicationSettings.Cast<string>());
            }

            if (PersonnalApplicationPathList != null)
            {
                string personalSettingsPath = System.IO.Path.Combine(localDirectory, "PersonnalApplicationPathList.txt");
                System.IO.File.WriteAllLines(personalSettingsPath, PersonnalApplicationPathList.Cast<string>());
            }
        }
    }    
}