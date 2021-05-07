using System;

namespace MailClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MailClient client = new MailClient();
            client.Connect();
            client.Send("Dit me may");
            //client.Quit();
        }
    }
}