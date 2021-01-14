using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPChat
{
    internal class Program
    {
        private static Network network;

        private static void Main(string[] args)
        {
            network = new Network();

            while (true)
            {
                network.Process();
            }
        }
    }
}

