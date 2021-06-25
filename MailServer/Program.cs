using System;

namespace MailServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Staring server on port 1578");
            MailServer server1 = new MailServer(1578);
            server1.Start();
            Console.WriteLine("Staring server on port 1579");
            MailServer server2 = new MailServer(1579);
            server2.Start();
        }
    }
}