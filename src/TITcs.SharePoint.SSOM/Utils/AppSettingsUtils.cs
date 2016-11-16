using System;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class AppSettingsUtils
    {
        #region fields and properties

        public static int CacheDurationInMinutes
        {
            get
            {
                try
                {
                    var minutes = ReadAppSettings("CacheDurationInMinutes");

                    return Convert.ToInt32(minutes);
                }
                catch
                {
                    return 5;
                }
            }
        }

        public const string SIGNALR_PATH = "app:SignalRPath";

        #endregion

        public static string Read(string key)
        {
            return ReadAppSettings(key);
        }


        private static string ReadAppSettings(string key)
        {
            object appSetting = System.Configuration.ConfigurationManager.AppSettings[key];

            if (appSetting == null)
                throw new Exception(string.Format("The key \"{0}\" in system.web/appSettings not found in web.config", key));

            return appSetting.ToString();
        }
    }
}
