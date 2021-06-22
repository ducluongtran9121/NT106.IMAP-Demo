using MailClient.Common;
using System.Collections.ObjectModel;

namespace MailClient.DataModels.Mail
{
    public class MailBox : BindableBase
    {
        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public ObservableCollection<MailMessage> Messages { get; set; } = new();
    }
}