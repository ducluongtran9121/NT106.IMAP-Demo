using MailClient.Common;
using System.Collections.ObjectModel;

namespace MailClient.DataModels.Mail
{
    public class Account : BindableBase
    {
        private string address = string.Empty;

        public string Address
        {
            get => address;
            set => SetProperty(ref address, value);
        }

        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string glyph = string.Empty;

        public string Glyph
        {
            get => glyph;
            set => SetProperty(ref glyph, value);
        }

        public ObservableCollection<MailBox> MailBoxes { get; set; } = new();

        public Account()
        {
            Name = string.Empty;
            Glyph = string.Empty;
            Address = string.Empty;
        }
    }
}