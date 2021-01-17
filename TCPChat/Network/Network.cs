using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace TCPChat
{
    public class Network
    {
        public User user;
        public CMD cmd;

        private Server server;
        private TcpClient client;

        private Thread ListenThread;
        private Thread ReceiveThread;

        private string id = "null";

        protected internal string host = "127.0.0.1";
        protected internal int port = 23;

        private bool isConnected = false;
        private bool isServer = false;
        private bool RecieveMessages = false;

        private NetworkStream stream;

        public Network()
        {
            cmd = new CMD();
        }

        public void Process()
        {
            ParseCommand(cmd.ReadLine(user.UserName));
        }

        protected internal void RegisterUser()
        {
            Console.Write("Enter your name: ");
            string userName = Console.ReadLine();
            Console.Title = userName;

            Console.Write("Enter your color (white): ");
            string color = Console.ReadLine();

            Console.Clear();

            user = new User(userName, ColorParser.GetColorFromString(color));
        }

        public void StartClient()
        {
            if (stream != null || client != null)
            {
                DisconnectClient();
            }

            if (isServer) isServer = false;

            try
            {
                client = new TcpClient(host, port);
                //client.Connect(host, port);
                isConnected = true;
                stream = client.GetStream();

                Message UserDataMessage = new Message(8, user);      //Send UserData to the server
                stream.Write(UserDataMessage.Serialize());

                cmd.WriteLine("Succesfully connected to the server");

                GetID();

                RecieveMessages = true;

                ReceiveThread = new Thread(new ThreadStart(ReceiveMessage));
                ReceiveThread.Start();
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't connect to the server: " + e.Message);
            }
        }

        public void StartServer()
        {
            if(server != null)
            {
                server.Disconnect();
                server = null;

                if (ListenThread != null)
                    ListenThread.Interrupt();
            }

            try
            {
                server = new Server(cmd, port);
                isServer = true;

                if (isConnected) isConnected = false;

                ListenThread = new Thread(new ThreadStart(server.Listen));
                ListenThread.Start();

                isServer = true;
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't start server: " + e.Message);
                return;
            }
        }

        private void ReceiveMessage()
        {
            byte[] message;

            while(RecieveMessages)
            {
                try
                {
                    message = GetMessage();

                    if (message != null && message.Length > 0)
                    {
                        Message msg = new Message(message);
                        ParseMessage(msg);
                    }
                }
                catch (ThreadInterruptedException) { return; }
                catch (System.IO.IOException) { return; }
                catch (Exception e)
                {
                    Console.WriteLine("Can't receive message: " + e.Message);

                }
            }
        }

        private byte[] GetMessage()
        {
            try
            {
                byte[] data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;

                if(stream != null)
                {
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data));
                    } while (stream.DataAvailable);

                    return Encoding.Unicode.GetBytes(builder.ToString());
                }

                return null;

            }
            catch (System.NullReferenceException) { return null; }

            catch (System.IO.IOException) 
            { 
                cmd.WriteLine("You are disconnetcted"); 
                cmd.SwitchToPrompt();
                if (isConnected) DisconnectClient();
                else if(isServer) DisconnectServer();
                return null;
            }

            catch (Exception e)
            {
                cmd.WriteLine("Can't get message from thread: " + e.Message);

                return null;
            }
        }

        /// <summary>
        /// Sends messages with PostCodes between 1-4
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="PostCode"></param>
        private void SendMessage(string msg, int PostCode)
        {
            try
            {
                if (msg.Trim().Length < 0) throw new Exception("Message is empty");
                Message message = new Message(PostCode, user, msg);
                byte[] data = message.Serialize();

                cmd.UserWriteLine(msg, user);
                stream.Write(data, 0, data.Length);
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't send message: " + e.Message);
            }
        }
        private void SendMessage(Message msg)
        {
            try
            {
                byte[] data = msg.Serialize();

                if(data.Length > 0)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't send message: " + e.Message);
            }
        }
        private void SendServerMessage(string msg, int PostCode)
        {
            try
            {
                if (msg.Trim().Length < 1) throw new Exception("Message is empty");
                Message message = new Message(PostCode, user, msg);

                cmd.UserWriteLine(msg, user);
                server.BroadcastFromServer(message);
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't send message from the server: " + e.Message);
            }
        }

        private void GetID()
        {
            Message msg = new Message(GetMessage());

            this.id = msg.message;
            cmd.WriteLine(id);
        }

        private void DisconnectClient()
        {
            Message msg = new Message(9, user);
            stream.Write(msg.Serialize());       //Disconnect this client from server

            if (RecieveMessages)
            {
                RecieveMessages = false;
                ReceiveThread.Interrupt();
                ReceiveThread = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client.Dispose();
                client = null;
            }

            isConnected = false;
        }
        private void DisconnectServer()
        {
            server.Disconnect();

            isServer = false;
        }

        private void ParseMessage(Message message)
        {
            switch (message.PostCode)
            {
                case 1:
                    cmd.UserWriteLine(message.message, message.Sender); break;
                case 8:
                    cmd.ConnectionMessage(message.Sender, "has joined"); break;
                case 9:
                    cmd.ConnectionMessage(message.Sender, "has disconnected"); break;
                case 10:
                    {
                        DisconnectClient();
                        cmd.WriteLine("Server was stoped");
                        break;
                    }
                default: return;
            }

            cmd.SwitchToPrompt();
        }

        private void ParseCommand(string command)
        {
            if (command.Trim().Length < 1) return;
            if (command[0] != '/')
            {
                if (isConnected)
                {
                    SendMessage(command, 1);
                    return;
                }
                else if (isServer)
                {
                    SendServerMessage(command, 1);
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
                            if ((client != null || stream != null) && isConnected)
                            {
                                cmd.WriteLine("You need to disconnect first");

                                return;
                            }

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
                                if (isConnected)
                                    DisconnectClient();
                                else if (isServer) DisconnectServer();
                                else return;
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

                    case string s when (s == "color"):
                        {
                            if (args.Length == 2)
                            {
                                if (args[1][0] != '#')
                                {
                                    user.SetColor(ColorParser.GetColorFromString(args[1]));

                                    if (isConnected)
                                    {
                                        Message update = new Message(7, user);
                                        SendMessage(update);
                                    }
                                }

                                break;
                            }
                            else return;
                        }
                }
            }
        }
    }
}
