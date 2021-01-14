using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPChat
{
    internal class Program
    {
        private static User user;
        private static CMD cmd;
        private static Server server;
        private static Thread ListenThread;
        private static Thread RecieveThread;

        private static string id = "null";

        private static string host = "127.0.0.1";
        private static int port = 23;
        private static TcpClient client;
        private static NetworkStream stream;
        private static bool isConnected = false;
        private static bool isServer = false;

        private static Network network;

        private static void RegisterUser()
        {
            Console.Write("Enter your name: ");
            string userName = Console.ReadLine();
            Console.Write("Enter your color (white): ");
            string color = Console.ReadLine();
            Console.Clear();

            user = new User(userName, ColorParser.GetColorFromString(color));
        }

        private static void StartServer()
        {
            try
            {
                server = new Server(cmd, port);
                isServer = true;
                ListenThread = new Thread(new ThreadStart(server.Listen));
                ListenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        private static void StartClient()
        {
            if (stream != null || client != null)
                Disconnect();

            client = new TcpClient();

            try
            {
                client.Connect(host, port);
                isConnected = true;
                stream = client.GetStream();
                cmd.WriteLine("Succesfull connected");

                Message message = new Message(8, user);

                byte[] data = message.Serialize();
                stream.Write(data, 0, data.Length);

                RecieveThread = new Thread(new ThreadStart(ReceiveMessage));
                RecieveThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isConnected = false;
                Disconnect();
            }
        }

        private static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleCancelKeyPressed;
            network = new Network();

            while (true)
            {
                network.Process();
            }
        }

        private static void SendFromServer(string message)
        {
            if (message.Length == 0) return;

            Message msg = new Message(1, user, message);
            cmd.UserWriteLine(message, user);
            server.BroadcastFromServer(msg);
        }

        private static void SendMessage(Message msg)
        {
            cmd.ParseMessage(msg);
            byte[] data = msg.Serialize();
            stream.Write(data);
        }

        private static void SendMessage(string message)
        {
            if (message.Length == 0) return;

            cmd.UserWriteLine(message, user);
            Message msg = new Message(1, user, message);

            byte[] data = msg.Serialize();
            stream.Write(data, 0, data.Length);
        }

        private static string GetMessage()
        {
            try
            {
                byte[] data = new byte[128];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;

                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (stream.DataAvailable);

                return builder.ToString();
            }
            catch(Exception e)
            {
                cmd.WriteLine("Cant send message" + e.Message);
                return null;
            }
        }

        private static void ReceiveMessage()
        {
            string message;

            while (true)
            {
                try
                {
                    message = GetMessage();

                    if (message.Length > 0)
                    {
                        Message msg = new Message(Encoding.Unicode.GetBytes(message));
                        cmd.ParseMessage(msg);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                catch (SocketException)
                {
                    return;
                }
                catch (System.IO.IOException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    cmd.WriteLine("Lost Connection: " + ex.Message);
                    Disconnect();
                }
            }
        }

        private static void ConsoleCancelKeyPressed()
        {
            Disconnect();
        }
        private static void ConsoleCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            ConsoleCancelKeyPressed();
        }

        private static void Disconnect()
        {
            if (RecieveThread != null)
            {
                RecieveThread.Interrupt();
                RecieveThread = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }

            isConnected = false;
        }

        private static void ParseCommand(string command)
        {
            if (command.Trim().Length < 1) return;
            if (command[0] != '/')
            {
                if (isConnected)
                {
                    SendMessage(command);
                    return;
                }
                else if (isServer)
                {
                    SendFromServer(command);
                    return;
                }
                else cmd.UserWriteLine(command, user);
            }
            else
            {
                command = command.ToLower();
                string[] args = command.Split(" ");
                args[0] = args[0].Substring(1);

                switch (args[0])
                {
                    case string s when (s == "join" || s == "connect"):
                        {
                            if (args.Length == 3)
                            {
                                host = args[1];
                                port = Convert.ToInt32(args[2]);
                            }
                            else if (args.Length == 2)
                            {
                                string[] data = args[1].Split(":");
                                host = data[0];
                                port = Convert.ToInt32(data[1]);
                            }
                            else return;

                            if (client != null || stream != null) SendMessage(new Message(9, user));
                            StartClient();
                            break;
                        }
                    case string s when (s == "create" || s == "room"):
                        {
                            if (args.Length == 2)
                            {
                                port = Convert.ToInt32(args[1]);
                            }
                            else return;

                            StartServer();
                            break;
                        }
                    case string s when (s == "disconnect" || s == "dconnect"):
                        {
                            if (args.Length == 1)
                            {
                                SendMessage(new Message(9, user));
                                Disconnect();
                            }
                            break;
                        }
                    case string s when (s == "clear" || s == "clr"):
                        {
                            if (args.Length == 1)
                            {
                                cmd.Clear();
                            }
                            break;
                        }
                }
            }
        }
    }
}

