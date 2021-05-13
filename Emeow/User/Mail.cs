using Emeow.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Emeow.User
{
    public class Mail: BindableBase
    {
        public ObservableCollection<ImapFlag> flags;

        public string AddressFrom { get; set; }

        public string AddressTo { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public string Text { get; set; }

        public string TextPreview { get; set; }

        public Mail()
        {

        }

        public Mail(List<string> header, string Text)
        {
            GetHeaderFromResponse(header);

            this.Text = Text;

            TextPreview = Text.Replace('\n', ' ').Substring(0, Text.Length > 50 ? 50 : Text.Length);
        }

        public void GetHeaderFromResponse(List<string> header)
        {
            Regex regex = new(pattern: @"[a-zA-Z0-9.a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9]+\.[a-zA-Z]+");

            Match match;

            match= regex.Match(header[0]);
            if (match.Success)
            {
                AddressFrom = match.Value;
            }

            match = regex.Match(header[1]);
            if (match.Success)
            {
                AddressTo = match.Value;
            }

            Subject = header[2].Replace(header[2].Substring(0, 9), "");
        }
    }

    public enum ImapFlag
    {
        Seen,
        Answered,
        Flagged,
        Deleted,
        Draft,
        Recent,
    }
}
