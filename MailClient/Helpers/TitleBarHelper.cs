using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.Helpers
{
    public class TitleBarHelper
    {
        public static string WindowTitle => Windows.ApplicationModel.Package.Current.DisplayName;
        public static double TitleBarHeight => Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.Height;
    }
}
