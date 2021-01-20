using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using TCPChat.AudioEngine;

namespace TCPChat
{
    public enum Input
    {
        Command,
        Message,
        Empty
    }

    internal class Program
    {
        private static NetworkManager network;

        private static CachedSound StartupSound;
        private static CachedSound MessageNotificationSound;
        private static CachedSound ConnectionSound;

        private static void Main(string[] args)
        {
            try
            {
                StartupSound = new CachedSound("Audio/Startup.mp3");
                MessageNotificationSound = new CachedSound("Audio/MessageNotification.mp3");
                ConnectionSound = new CachedSound("Audio/Connection.mp3");
            }
            catch { }

            Console.OutputEncoding = Encoding.Unicode;
            network = new NetworkManager(Notification);

            string name = "";
            string color = "";

            int port = 0;
            string host = "";

            bool StartServer = false;
            bool StartClient = false;

            try
            {
                AudioPlaybackEngine.Instance.PlaySound(StartupSound);
            } catch { }

            if (args.Length > 0)                //Check for console options
            {
                for (int i = 0; i < args.Length - 1; i++)       
                {
                    string arg = args[i];

                    switch (arg)
                    {
                        case string s when (s == "-N" || s == "--name"):
                            {
                                if (i + 1 < args.Length)        //If after option we have argument
                                {
                                    name = args[i + 1];         //Set up it
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
                                    if (args[i + 1].Contains(':'))                  //If host and port in format [host:port]
                                    {
                                        string[] HostArgs = args[i + 1].Split(':'); //Split it

                                        host = HostArgs[0];
                                        Int32.TryParse(HostArgs[1], out port);      //And convert

                                        i++;

                                        StartServer = false;
                                        StartClient = true;
                                    }
                                    else if (i + 2 < args.Length)                   //If there is 2 arguments and format [host] [port]
                                    {
                                        host = args[i + 1];
                                        Int32.TryParse(args[i + 2], out port);      //Convert it
                                        i += 2;

                                        continue;
                                    }
                                }
                                break;
                            }

                    }
                }

                if (color.Length > 0 && name.Length > 0)                            //If color and name are specified then
                {
                    network.user = new User(name, ColorParser.GetColorFromString(color)); //Set up user

                    network.cmd.Clear();

                    if (host.Length > 0 && port > 0 && StartClient && !StartServer)     //If host and port are specified and this is client
                    {
                        network.host = host;
                        network.port = port;

                        if(network.StartClient())                      //then start client
                        {
                            try
                            {
                                AudioPlaybackEngine.Instance.PlaySound(ConnectionSound);
                            } catch { }
                        }
                        network.cmd.SwitchToPrompt();
                    }

                    if (port > 0 && StartServer && !StartClient)    //If port specified and this is server
                    {
                        network.port = port;

                        if(network.StartServer())                    //Start server
                        {
                            try
                            {
                                AudioPlaybackEngine.Instance.PlaySound(ConnectionSound);
                            }
                            catch { }
                        }

                        network.cmd.SwitchToPrompt();
                    }
                }
                else network.RegisterUser();                                        //Or register him

            }

            else network.RegisterUser();                //If there are no commands, register user
            while (true)
            {
                ParseCommand(network.Process());                      //Starts manager
            }
        }
        private static void ParseCommand(string command)
        {
            Input input = network.GetInputType(command);

            if (input == Input.Message)                          //If this is not a command
            {
                network.SendMessage(command);
            }
            else if(input == Input.Command)
            {
                string[] args = network.GetCommandArgs(command);

                switch (args[0])
                {
                    case string s when (s == "join" || s == "connect"):
                        {
                            if (network.IsConnectedToServer())  //So if you try reconnect and you already have sesson
                            {
                                network.cmd.WriteLine("You need to disconnect first");          //You need ro disconnect)

                                return;
                            }

                            if(network.TryJoin(args))
                            {
                                try
                                {
                                    AudioPlaybackEngine.Instance.PlaySound(ConnectionSound);
                                }
                                catch { }
                            }

                            break;
                        }
                    case string s when (s == "create" || s == "room"):
                        {
                            if (args.Length == 2)
                            {
                                if(network.TryCreateRoom(args[1]))
                                {
                                    try
                                    {
                                        AudioPlaybackEngine.Instance.PlaySound(ConnectionSound);
                                    }
                                    catch { }
                                }
                            }
                            else return;

                            break;
                        }
                    case string s when (s == "disconnect" || s == "dconnect"):
                        {
                            if (args.Length == 1)
                            {
                                network.TryDisconnect();
                            }
                            break;
                        }
                    case string s when (s == "clear" || s == "clr"):
                        {
                            if (args.Length == 1)
                            {
                                network.cmd.Clear();
                            }
                            break;
                        }

                    case string s when (s == "color"):
                        {
                            if (args.Length == 2)
                            {
                                network.user.SetColor(ColorParser.GetColorFromString(args[1]));
                                network.UpdateUserData();

                                break;
                            }

                            else return;
                        }
                }
            }
        }

        private static void Notification()
        {
            AudioPlaybackEngine.Instance.PlaySound(MessageNotificationSound);
        }
    }
}

