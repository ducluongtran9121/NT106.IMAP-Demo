using System;

namespace MailServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Staring server on port 1578");
            MailServer server = new MailServer(1578);
            server.Start();
        }
    }
}