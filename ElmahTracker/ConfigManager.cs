using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ElmahTracker
{
    public static class ConfigManager
    {
        public static string AppDirectory { get; private set; }
        public static string ConfigFilePath { get; private set; }
        public static string LatestItemFilePath { get; private set; }

        public static Action ConfigSave;

        private static AppConfig _config;
        public static AppConfig Config
        {
            get
            {
                if (_config == null)
                {
                    SetupEnvironment();

                    LoadConfig();
                }
                return _config;
            }
            set { _config = value; }
        }

        private static void LoadConfig()
        {
            using (FileStream stream = File.OpenRead(ConfigFilePath))
            {
                _config = new DataContractJsonSerializer(typeof(AppConfig)).ReadObject(stream) as AppConfig;
            }
        }

        private static void SetupEnvironment()
        {
            AppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CE\Elmah Tracker");
            ConfigFilePath = Path.Combine(AppDirectory, @"config.json");
            LatestItemFilePath = Path.Combine(AppDirectory, @"latestitem.json");

            if (!Directory.Exists(AppDirectory))
            {
                Directory.CreateDirectory(AppDirectory);
            }

            if (!File.Exists(ConfigFilePath))
            {
                SaveConfig(string.Empty);
            }
        }

        public static void SaveConfig(string rssUrl, int interval = 15, bool stayOnTop = false)
        {
            // clear latest item if rss url changed
            if (_config != null && rssUrl != _config.RssUrl)
            {
                if (File.Exists(LatestItemFilePath))
                {
                    File.Delete(LatestItemFilePath);
                }
            }

            _config = new AppConfig
            {
                RssUrl = rssUrl,
                Interval = interval,
                StayOnTop = stayOnTop
            };

            using (FileStream stream = File.Create(ConfigFilePath))
            {
                new DataContractJsonSerializer(typeof(AppConfig)).WriteObject(stream, _config);
            }

            if (ConfigSave != null)
            {
                ConfigSave();
            }
        }

        #region Nested type: AppConfig

        public class AppConfig
        {
            public string RssUrl { get; set; }
            public int Interval { get; set; }
            public bool StayOnTop { get; set; }
        }

        #endregion
    }
}