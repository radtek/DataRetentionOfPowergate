using System;
using System.Collections.Generic;

namespace DataRetention.Core.Infrastructure
{
    public class NewTaskRunResult
    {
        public bool RunRequired { get; set; }

        public string SessionId { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        // note that other custom settings may be provided here which some robots will need.
        // WARNING: this is not yet fleshed out - but an example of what it may look like it below....
        private AdditionalSettings _additionalSettings = new AdditionalSettings();
        public AdditionalSettings AdditionalSettings
        {
            get { return _additionalSettings; }
            set { _additionalSettings = value; }
        }
    }

    public class AdditionalSettings
    {
        private Dictionary<string, dynamic> _settings = new Dictionary<string, dynamic>();
        public Dictionary<string, dynamic> Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public T GetSettingAs<T>(string key)
        {
            return _settings.ContainsKey(key) && _settings[key] is T ? _settings[key] : null;
        }

        public string GetSettingAsString(string key)
        {
            return GetSettingAs<string>(key);
        }

        public int? GetSettingAsInt(string key)
        {
            return GetSettingAs<int>(key);
        }

        public bool? GetSettingAsBool(string key)
        {
            return GetSettingAs<bool>(key);
        }

        public DateTime? GetSettingAsDateTime(string key)
        {
            return GetSettingAs<DateTime>(key);
        }
    }
}