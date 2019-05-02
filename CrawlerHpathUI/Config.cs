using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Configuration;


namespace CrawlerHpathUI
{
    public class Config
    {
        private static string KEY_LAST_OPEN = "LastOpened";

        private static string KEY_LAST_SEARCH = "LastSearch";

        private static object substitute = new object();
        private static Config _singleton = null;

        /// <summary>
        /// Returns the only instance of the configuration reader.
        /// </summary>
        public static Config Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new Config();

                return _singleton;
            }
        }

        #region Instance Members
        virtual public string LastOpened
        {
            get
            {
                return getValue(KEY_LAST_OPEN, @"http://www.fftoday.com");
            }
            set
            {
                setValue(KEY_LAST_OPEN, value);
            }
        }

        virtual public string LastSearch
        {
            get
            {
                return getValue(KEY_LAST_SEARCH, string.Empty);
            }
            set
            {
                setValue(KEY_LAST_SEARCH, value);
            }
        }
        


        public event EventHandler ValueChanged;



        string getValue(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                setValue(key, defaultValue);
                return defaultValue;
            }
            else 
            {
                return value;
            }
        }

        void setValue(string key, string value)
        {
            string old = null;
            lock (key)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] == null)
                    config.AppSettings.Settings.Add(key, value);
                else
                {
                    old = config.AppSettings.Settings[key].Value;
                    config.AppSettings.Settings[key].Value = value;
                }
                if (old != value)
                {
                    config.Save(ConfigurationSaveMode.Modified);
                    if (ValueChanged != null)
                        ValueChanged(typeof(Config), EventArgs.Empty);
                }
            }
        }

        #endregion Instance members
    }
}
