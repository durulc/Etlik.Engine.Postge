using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace App.Common.Configurations
{
    public class ConfigManager : IConfigManager, IDisposable
    {
        readonly Dictionary<string, string> _configuration = new Dictionary<string, string>();
        bool _disposed = false;

        readonly NameValueCollection _appSettings = ConfigurationManager.AppSettings;
        private void Initalize()
        {
            foreach (var key in _appSettings.AllKeys)
            {
                if (!this._configuration.ContainsKey(key))
                {
                    try
                    {
                        this._configuration.Add(key, _appSettings[key]);
                    }
                    catch { }
                }
            }
        }

        private static ConfigManager Config { get; set; }
        public static string GetValue(string key)
        {
            if (Config == null) { Config = new ConfigManager(); }
            return Config.GetConfigurationSettingValue(key);
        }

        public static void Init()
        {
            if (Config == null) { Config = new ConfigManager(); }
            Config.Initalize();
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
