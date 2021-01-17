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
            Console.OutputEncoding = Encoding.Unicode;
            network = new Network();

            string name = "";
            string color = "";

            int port = 0;
            string host = "";

            bool StartServer = false;
            bool StartClient = false;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    string arg = args[i];

                    switch (arg)
                    {
                        case string s when (s == "-N" || s == "--name"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    name = args[i + 1];
                                    i++;

                                    continue;
                                }
                                break;
                            }
                        case string s when (s == "-c" || s == "--color"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    color = args[i + 1];
                                    i++;

                                    continue;
                                }
                                break;
                            }
                        case string s when (s == "-S" || s == "--server"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    Int32.TryParse(args[i + 1], out port);
                                    i++;

                                    StartServer = true;

                                    continue;
                                }
                                break;
                            }
                        case string s when (s == "-C" || s == "--client"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    if (args[i + 1].Contains(':'))
                                    {
                                        string[] HostArgs = args[i + 1].Split(':');

                                        host = HostArgs[0];
                                        Int32.TryParse(HostArgs[1], out port);

                                        i++;

                                        StartServer = false;
                                        StartClient = true;
                                    }
                                    else if (i + 2 < args.Length)
                                    {
                                        host = args[i + 1];
                                        Int32.TryParse(args[i + 2], out port);
                                        i += 2;

                                        continue;
                                    }
                                }
                                break;
                            }

                    }
                }

                if (color.Length > 0 && name.Length > 0)
                {
                    network.user = new User(name, ColorParser.GetColorFromString(color));
                }
                else network.RegisterUser();

                if (host.Length > 0 && port > 0 && StartClient && !StartServer)
                {
                    network.host = host;
                    network.port = port;

                    network.StartClient();
                }
                if (port > 0 && StartServer && !StartClient)
                {
                    network.port = port;

                    network.StartServer();
                }

            }

            else network.RegisterUser();

            while (true)
            {
                network.Process();
            }
        }
    }
}

