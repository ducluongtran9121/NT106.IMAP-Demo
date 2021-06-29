using Windows.Storage;

namespace MailClient.Helpers
{
    public class SettingsHelper
    {
        private const string IsFirstTimeLoginKey = "IsFirstTimeLogin";

        public static bool IsFirstTimeLogin
        {
            get
            {
                object valueFromSettings = ApplicationData.Current.LocalSettings.Values[IsFirstTimeLoginKey];
                if (valueFromSettings == null)
                {
                    ApplicationData.Current.LocalSettings.Values[IsFirstTimeLoginKey] = false;
                    valueFromSettings = false;
                }
                return (bool)valueFromSettings;
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[IsFirstTimeLoginKey] = value;
            }
        }

        private const string IsUseTLSKey = "IsUseTLS";

        public static bool IsUseTLS
        {
            get
            {
                object valueFromSettings = ApplicationData.Current.LocalSettings.Values[IsUseTLSKey];
                if (valueFromSettings == null)
                {
                    ApplicationData.Current.LocalSettings.Values[IsUseTLSKey] = false;
                    valueFromSettings = false;
                }
                return (bool)valueFromSettings;
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[IsUseTLSKey] = value;
            }
        }
    }
}