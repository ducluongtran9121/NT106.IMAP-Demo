using MailClient.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
