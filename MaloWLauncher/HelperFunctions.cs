using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MaloWLauncher
{
    class HelperFunctions
    {
        private static readonly List<string> WHITELISTED_DLC_FOLDERS = new List<string>(new string[] 
        {
            // Default civ5 folders
            "DLC_01",
            "DLC_02",
            "DLC_03",
            "DLC_04",
            "DLC_05",
            "DLC_06",
            "DLC_07",
            "DLC_Deluxe",
            "DLC_SP_Maps",
            "DLC_SP_Maps_2",
            "DLC_SP_Maps_3",
            "Expansion",
            "Expansion2",
            "Shared",
            "Tablet",

            // EUI folder
            "UI_bc1",
        });

        public static void UpdateLaunchParameters(string launchString)
        {
            ConfigFile configFile = ReadConfigFile();
            configFile.launchParameters = launchString;
            File.WriteAllText(@"config.txt", JsonConvert.SerializeObject(configFile, Formatting.Indented));
        }

        public static void AddExpandedModToConfig(string modName)
        {
            ConfigFile configFile = ReadConfigFile();
            if(configFile.expandedMods.Contains(modName))
            {
                return;
            }
            configFile.expandedMods.Add(modName);
            File.WriteAllText(@"config.txt", JsonConvert.SerializeObject(configFile, Formatting.Indented));
        }

        public static void RemoveExpandedModToConfig(string modName)
        {
            ConfigFile configFile = ReadConfigFile();
            if (!configFile.expandedMods.Contains(modName))
            {
                return;
            }
            configFile.expandedMods.Remove(modName);
            File.WriteAllText(@"config.txt", JsonConvert.SerializeObject(configFile, Formatting.Indented));
        }

        public static bool IsModExpanded(string modName)
        {
            ConfigFile configFile = ReadConfigFile();
            return configFile.expandedMods.Contains(modName);
        }

        public static string GetLaunchParameters()
        {
            ConfigFile configFile = ReadConfigFile();
            return configFile.launchParameters;
        }

        public static void InstallMod(ModModel mod)
        {
            ConfigFile configFile = ReadConfigFile();
            string dlcFolder = configFile.gameLocation + @"\Assets\DLC\";
            string[] subDirectories = Directory.GetDirectories(dlcFolder);
            foreach (string subdirectory in subDirectories)
            {
                if(!WHITELISTED_DLC_FOLDERS.Contains(subdirectory.Split('\\').Last()))
                {
                    Directory.Delete(subdirectory, true);
                }
            }

            DataFile dataFile = new DataFile();
            if (mod != null)
            {
                dataFile.installedMod = mod.GetFullName();
                DirectoryCopy(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\mods\" + mod.GetFullName(), dlcFolder);
            }
            else
            {
                dataFile.installedMod = null;
            }

            File.WriteAllText(@"data.txt", JsonConvert.SerializeObject(dataFile, Formatting.Indented));
        }

        public static void LaunchCiv5()
        {
            ConfigFile configFile = ReadConfigFile();
            ProcessStartInfo civ5 = new ProcessStartInfo();
            civ5.FileName = configFile.gameLocation + @"\Launcher.exe";
            civ5.Arguments = configFile.launchParameters;
            civ5.WorkingDirectory = Path.GetDirectoryName(configFile.gameLocation + @"\Launcher.exe");
            Process.Start(civ5);
        }

        public static ConfigFile ReadConfigFile()
        {
            if (!File.Exists(@"config.txt"))
            {
                // Create a default config file with default values if it does not exist
                ConfigFile configFile = new ConfigFile();
                File.WriteAllText(@"config.txt", JsonConvert.SerializeObject(configFile, Formatting.Indented));
            }
            return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(@"config.txt"));
        }

        public static DataFile ReadDataFile()
        {
            if (!File.Exists(@"data.txt"))
            {
                // Create a default data file with no mods installed.
                DataFile dataFile = new DataFile();
                File.WriteAllText(@"data.txt", JsonConvert.SerializeObject(dataFile, Formatting.Indented));
            }
            return JsonConvert.DeserializeObject<DataFile>(File.ReadAllText(@"data.txt"));
        }

        public static bool IsModDownloaded(String modName)
        {
            string modsFolder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\mods\";
            if (!Directory.Exists(modsFolder))
            {
                Directory.CreateDirectory(modsFolder);
            }
            List<string> modsPaths = Directory.GetDirectories(modsFolder).ToList();
            foreach (string modPath in modsPaths)
            {
                string m = modPath.Split('\\').Last();
                if (modName == m)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsModInstalled(String modName)
        {
            return (modName == ReadDataFile().installedMod);
        }

        public static ModList GetModsListFromServer()
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(Globals.SERVER_URL);
                return JsonConvert.DeserializeObject<ModList>(json);
            }
        }

        public static void OpenFileBrowserAndSetConfigGameLocation()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "Civ5|Launcher.exe"
            };
            dlg.ShowDialog();
            string gameLocation = dlg.FileName.Replace("\\Launcher.exe", "");
            ConfigFile configFile = ReadConfigFile();
            configFile.gameLocation = gameLocation;
            File.WriteAllText(@"config.txt", JsonConvert.SerializeObject(configFile, Formatting.Indented));
        }

        public static bool IsConfigGameLocationValid()
        {
            ConfigFile configFile = ReadConfigFile();
            if(configFile.gameLocation == null || configFile.gameLocation == "")
            {
                return false;
            }
            DirectoryInfo dir = new DirectoryInfo(configFile.gameLocation);
            return dir.Exists;
        }

        public static bool IsNewVersionRequired(string version)
        {
            String[] serverVersions = version.Split('.');
            String[] clientVersions = Globals.VERSION.Split('.');
            // Only Major/Minor version requires new client version.
            if(serverVersions[0] != clientVersions[0] || serverVersions[1] != clientVersions[1])
            {
                return true;
            }
            return false;
        }
        
        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
            
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
        }
    }
}
