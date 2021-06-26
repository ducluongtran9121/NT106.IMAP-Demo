using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Net;

namespace Proxy
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Start proxy on port 1578");
            var coreComm = new CoreComm();
            var dispatcher = new Dispatcher();
            dispatcher.ListenForRequests();
        }
    }
}
