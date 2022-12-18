namespace App.Common.Configurations
{
    public interface IConfigManager
    {
        string GetConfigurationSettingValue(string configurationSettingName);
        string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue);
    }
}
