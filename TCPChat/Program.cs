using System;
using System.Text;

using TCPChat.AudioEngine;
using TCPChat.Messages;
using TCPChat.Network;
using TCPChat.Tools;

namespace TCPChat
{
    internal static class Program
    {
        private static NetworkManager _network;

        private static CachedSound _startupSound;
        private static CachedSound _messageNotificationSound;
        private static CachedSound _connectionSound;

        private static int Main(string[] args)
        {
            _startupSound = Sound.TryLoadCached("Audio/Startup.mp3");
            _messageNotificationSound = Sound.TryLoadCached("Audio/MessageNotification.mp3");
            _connectionSound = Sound.TryLoadCached("Audio/Connection.mp3");

            Console.OutputEncoding = Encoding.Unicode;
            _network = new NetworkManager(Notification);

            var name = "";
            var color = "";

            var port = 0;
            var host = "";

            var startServer = false;
            var startClient = false;


            _startupSound?.TryPlay();

            if (args.Length > 0)                //Check for console options
            {
                for (var i = 0; i < args.Length - 1; i++)       
                {
                    var arg = args[i];

                    switch (arg)
                    {
                        case { } s when (s == "-N" || s == "--name"):
                            {
                                if (i + 1 < args.Length)        //If after option we have argument
                                {
                                    name = args[i + 1];         //Set up it
                                    i++;
                                }
                                break;
                            }
                        case { } s when (s == "-c" || s == "--color"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    color = args[i + 1];
                                    i++;
                                }
                                break;
                            }
                        case { } s when (s == "-S" || s == "--server"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    int.TryParse(args[i + 1], out port);
                                    i++;

                                    startServer = true;
                                }
                                break;
                            }
                        case { } s when (s == "-C" || s == "--client"):
                            {
                                if (i + 1 < args.Length)
                                {
                                    if (args[i + 1].Contains(':'))                  //If host and port in format [host:port]
                                    {
                                        var hostArgs = args[i + 1].Split(':'); //Split it

                                        host = hostArgs[0];
                                        int.TryParse(hostArgs[1], out port);      //And convert

                                        i++;

                                        startServer = false;
                                        startClient = true;
                                    }
                                    else if (i + 2 < args.Length)                   //If there is 2 arguments and format [host] [port]
                                    {
                                        host = args[i + 1];
                                        int.TryParse(args[i + 2], out port);      //Convert it
                                        i += 2;
                                    }
                                }
                                break;
                            }

                    }
                }

                if (color.Length > 0 && name.Length > 0)                            //If color and name are specified then
                {
                    _network.User = new User(name, ColorParser.GetColorFromString(color)); //Set up user

                    _network.Cmd.Clear();

                    if (host.Length > 0 && port > 0 && startClient && !startServer)     //If host and port are specified and this is client
                    {
                        _network.Connector.Host = host;
                        _network.Connector.Port = port;

                        if(_network.StartClient())                      //then start client
                        {
                            _connectionSound.TryPlay();
                        }
                        _network.Cmd.SwitchToPrompt();
                    }

                    if (port > 0 && startServer && !startClient)    //If port specified and this is server
                    {
                        _network.Connector.Port = port;

                        if(_network.StartServer())                    //Start server
                        {
                            _connectionSound.TryPlay();
                        }

                        _network.Cmd.SwitchToPrompt();
                    }
                }
                else _network.RegisterUser();                                        //Or register him

            }

            else _network.RegisterUser();                //If there are no commands, register user
            while (true)
            {
                ParseCommand(_network.Process());                      //Starts manager
            }
            // ReSharper disable once FunctionNeverReturns
        }
        private static void ParseCommand(string command)
        {
            var input = _network.GetInputType(command);

            switch (input)
            {
                //If this is not a command
                case Input.Message:
                    _network.SendMessage(new SimpleMessage(_network.User, command));
                    break;
                case Input.Command:
                {
                    string[] args = _network.GetCommandArgs(command);

                    switch (args[0])
                    {
                        case { } s when (s == "join" || s == "connect"):
                        {
                            if (_network.IsConnectedToServer())  //So if you try reconnect and you already have session
                            {
                                _network.Cmd.WriteLine("You need to disconnect first");          //You need ro disconnect)

                                return;
                            }

                            if(_network.TryJoin(args))
                            {
                                _connectionSound.TryPlay();
                            }

                            break;
                        }
                        case { } s when (s == "create" || s == "room"):
                        {
                            if (args.Length == 2)
                            {
                                if(_network.TryCreateRoom(args[1]))
                                {
                                    _connectionSound?.TryPlay();
                                }
                            }

                            break;
                        }
                        // ReSharper disable once StringLiteralTypo
                        case { } s when (s == "disconnect" || s == "dconnect"):
                        {
                            if (args.Length == 1)
                            {
                                _network.TryDisconnect();
                            }
                            break;
                        }
                        case { } s when (s == "clear" || s == "clr"):
                        {
                            if (args.Length == 1)
                            {
                                _network.Cmd.Clear();
                            }
                            break;
                        }

                        case { } s when (s == "color"):
                        {
                            if (args.Length != 2) return;
                            _network.User.SetColor(ColorParser.GetColorFromString(args[1]));
                            _network.SendMessage(new UserDataMessage(Method.Send, _network.User));

                            break;
                        }

                            case { } s when (s == "hash"):
                                {
                                    if(args.Length == 1)
                                    {
                                        _network.Cmd.WriteLine(VersionVerifier.GetStringHash());
                                    }
                                    break;
                                }
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Notification()
        {
            _messageNotificationSound.TryPlay();
        }
    }
}

