using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace RTLS.Manager
{
    public class ConfigManager : IConfigManager, IDisposable
    {
        private static ConfigManager Config { get; set; }

        readonly Dictionary<string, string> _configuration = new Dictionary<string, string>();
        bool _disposed = false;

        private NameValueCollection _appSettings = ConfigurationManager.AppSettings;
        private void Initalize()
        {
            _appSettings = ConfigurationManager.AppSettings;
            ConfigurationManager.RefreshSection("appSettings");
            foreach (var key in _appSettings.AllKeys)
            {
                if (!this._configuration.ContainsKey(key))
                {
                    try
                    {
                        this._configuration.Add(key, _appSettings[key]);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    this._configuration[key] = _appSettings[key];
                }
            }
        }
        
        public static string GetValue(string key)
        {
            if (Config == null) { Config = new ConfigManager(); }
            return Config.GetConfigurationSettingValue(key);
        }

        public static void Init()
        {
            if (Config == null) { Config = new ConfigManager(); }
            Config.Initalize();
            ConfigurationChangedEventArgs();
        }

        private static FileSystemWatcher _fileSystemWatcher;
        private static void ConfigurationChangedEventArgs()
        {
            NotifyFilters notifyFilters = NotifyFilters.LastWrite;
            string assemblyDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            _fileSystemWatcher = new FileSystemWatcher()
            {
                Path = assemblyDirectory,
                NotifyFilter = notifyFilters,
                Filter = "*.config"
            };
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private static void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                Config.Initalize();
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            return this.GetConfigurationSettingValueOrDefault(configurationSettingName, string.Empty);
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            if (!this._configuration.ContainsKey(configurationSettingName))
            {
                try
                {
                    this._configuration.Add(configurationSettingName, _appSettings[configurationSettingName]);
                }
                catch { }
            }

            return this._configuration[configurationSettingName];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }
        ~ConfigManager()
        {
            Dispose(false);
        }
    }
}
