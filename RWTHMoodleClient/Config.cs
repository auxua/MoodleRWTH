using System;
using System.Collections.Generic;
using System.Text;

namespace auxua.RWTHMoodleClient
{
    /// <summary>
    /// Central Configuration
    /// </summary>
    public class Config
    {
        internal const string OAuthEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/code";

        internal const string OAuthTokenEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/token";

        /// <summary>
        /// Add your Developer Client ID in here
        ///     Please remember: Never commit your ID!
        /// </summary>
        public static string ClientID = "";

        internal const string MoodleEndPoint = "https://moped.ecampus.rwth-aachen.de/proxy/api/v2/eLearning/Moodle";

        internal const string OAuthTokenInfoEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/tokeninfo";

        #region Token Management

        /*
         * If you want to store Tokens persistently (e.g., Device Storage in mobile App)
         * You can override the static properties with the
         * Device-specific persistent implementation
         * 
         * 
         **/

        internal static string AccessToken
        {
            get
            {
                return ConfigStorage.GetValue(nameof(AccessToken));
            }
            set
            {
                ConfigStorage.SetValue(nameof(AccessToken), value);
            }
        }
        internal static string RefreshToken
        {
            get
            {
                return ConfigStorage.GetValue(nameof(RefreshToken));
            }
            set
            {
                ConfigStorage.SetValue(nameof(RefreshToken), value);
            }
        }
        internal static string DeviceToken
        {
            get
            {
                return ConfigStorage.GetValue(nameof(DeviceToken));
            }
            set
            {
                ConfigStorage.SetValue(nameof(DeviceToken), value);
            }
        }

        /// <summary>
        /// The Storage method for Config to be used
        /// Set this to your persistent storage implementation
        /// </summary>
        public static IConfigStorage ConfigStorage { get; set; } = new DefaultConfigStorage();

        #endregion
    }

    /// <summary>
    /// BAsic interface for (Config) Data Storage
    /// </summary>
    public interface IConfigStorage
    {
        /// <summary>
        /// Gets the value of a config element
        /// </summary>
        string GetValue(string key);

        /// <summary>
        /// Sets a new Value for a config element
        /// </summary>
        void SetValue(string key, string newValue);
    }

    /// <summary>
    /// Default Storage for Config.
    /// Data is stored in memory and lost after app is closed
    /// </summary>
    public class DefaultConfigStorage : IConfigStorage
    {
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        internal DefaultConfigStorage()
        {
            dict.Add("AccessToken", "");
            dict.Add("RefreshToken", "");
            dict.Add("DeviceToken", "");
        }

        /// <summary>
        /// Gets Value from memory
        /// </summary>
        public string GetValue(string key) => dict[key];

        /// <summary>
        /// Sets new value in the memory
        /// </summary>
        public void SetValue(string key, string newValue)
        {
            dict[key] = newValue;
        }
    }
}
