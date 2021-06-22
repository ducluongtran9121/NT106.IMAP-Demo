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
    }
}