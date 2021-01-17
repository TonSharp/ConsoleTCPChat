using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace TCPChat
{
    public class NetworkManager
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

        public NetworkManager()
        {
            cmd = new CMD();
        }

        public void Process()
        {
            ParseCommand(cmd.ReadLine(user));      //Always read command and parse it in main thread
        }

        protected internal void RegisterUser()
        {
            Console.Write("Enter your name: ");
            string userName = Console.ReadLine();
            Console.Title = userName;             //Set title for user with him userName

            Console.Write("Enter your color (white): ");
            string color = Console.ReadLine();

            Console.Clear();

            user = new User(userName, ColorParser.GetColorFromString(color));   //Parse color from string and create user
        }

        public void StartClient()
        {
            if (stream != null || client != null)       //If the previous session was not closed
            {
                DisconnectClient();                     //Close it
            }

            if (isServer)                               //If it was Server
            {                     
                DisconnectServer();                     //Close it
            }

            try
            {
                client = new TcpClient(host, port);
                isConnected = true;
                stream = client.GetStream();            //Connects to the server and gets its stream

                Message UserDataMessage = new Message(8, user);      //Sends UserData to the server
                stream.Write(UserDataMessage.Serialize());

                cmd.WriteLine("Succesfully connected to the server");

                GetID();                                //Gets own ID

                RecieveMessages = true;

                ReceiveThread = new Thread(new ThreadStart(ReceiveMessage)); //Starting receive message thread
                ReceiveThread.Start();
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't connect to the server: " + e.Message);
            }
        }

        public void StartServer()
        {
            if(server != null)                  //If server was already started
            {
                server.Disconnect();            //Then close it
                server = null;

                if (ListenThread != null)
                    ListenThread.Interrupt();
            }

            try
            {
                server = new Server(cmd, port); //Start new server
                isServer = true;

                if (isConnected) isConnected = false;   //If it was client. No, its doesnt)

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
                    message = GetMessage();                     //Lets get message while Receive is available

                    if (message != null && message.Length > 0)  //If message is not empty
                    {
                        Message msg = new Message(message);     //Lets get it
                        ParseMessage(msg);                      //And parse
                    }
                }
                catch (ThreadInterruptedException) { return; }
                catch (System.IO.IOException) { return; }       //If Thread was interupted or something
                catch (Exception e)
                {
                    Console.WriteLine("Can't receive message: " + e.Message);   //If something went wrong

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

                if(stream != null)                          //If stream is not empty
                {
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data));   
                    } while (stream.DataAvailable);         //Lets read this while stream is available

                    return Encoding.Unicode.GetBytes(builder.ToString());   //Return message bytes array
                }

                return null;

            }
            catch (System.NullReferenceException) { return null; }

            catch (System.IO.IOException)                       //If stram was stopped
            { 
                cmd.WriteLine("You are disconnetcted"); 
                cmd.SwitchToPrompt();
                if (isConnected) DisconnectClient();
                else if(isServer) DisconnectServer();
                return null;
            }

            catch (Exception e)                                 //If something went wrong
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
                if (msg.Trim().Length < 0) throw new Exception("Message is empty"); //If message empty
                Message message = new Message(PostCode, user, msg);
                byte[] data = message.Serialize();

                cmd.UserWriteLine(msg, user);
                stream.Write(data, 0, data.Length);         //Serialize message and send it
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't send message: " + e.Message);  //If something went wrong
            }
        }
        private void SendMessage(Message msg)
        {
            try
            {
                byte[] data = msg.Serialize();

                if(data.Length > 0)             //If message is not empty
                {
                    stream.Write(data, 0, data.Length); //Send it
                }
            }
            catch(Exception e)
            {
                cmd.WriteLine("Can't send message: " + e.Message);  //If something went wrong ))
            }
        }
        private void SendServerMessage(string msg, int PostCode)
        {
            try
            {
                if (msg.Trim().Length < 1) throw new Exception("Message is empty");  //The same situation
                Message message = new Message(PostCode, user, msg);

                cmd.UserWriteLine(msg, user);
                server.BroadcastFromServer(message);                            //Broadcast message to all clients
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
            cmd.WriteLine("Your ID: "+ id);
        }

        private void DisconnectClient()
        {
            Message msg = new Message(9, user);  //Send message to server about disconnecting
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

            isConnected = false;        //Dispose everything
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
                        DisconnectClient();                 //If server sends us message about stopping
                        cmd.WriteLine("Server was stoped"); //We are decide to write this
                        break;
                    }
                default: return;
            }

            cmd.SwitchToPrompt();                           //Lets go back to console
        }

        private void ParseCommand(string command)
        {
            if (command.Trim().Length < 1) return;          //If command is empty return back
            if (command[0] != '/')                          //If this is not a command
            {
                if (isConnected)                            //If we are connected to the server
                {
                    SendMessage(command, 1);                //Send him message
                    return;
                }
                else if (isServer)                          //Or if we are a server
                {
                    SendServerMessage(command, 1);          //Broadcast this message
                    return;
                }
                else cmd.UserWriteLine(command, user);      //Or just print this message on the screen
            }
            else
            {
                command = command.ToLower();                //If this a command set it to lowercase
                string[] args = command.Split(" ");         //Split for arguments
                args[0] = args[0].Substring(1);             //And remove slash (/)

                switch (args[0])
                {
                    case string s when (s == "join" || s == "connect"):
                        {
                            if ((client != null || stream != null) && isConnected)  //So if you try reconnect and you already have sesson
                            {
                                cmd.WriteLine("You need to disconnect first");          //You need ro disconnect)

                                return;
                            }

                            if (args.Length == 3)                                   //If format is /join [host] [port]
                            {
                                host = args[1];
                                Int32.TryParse(args[2], out port);
                            }
                            else if (args.Length == 2)                              //If format is /join [host]:[port]
                            {
                                string[] data = args[1].Split(":");
                                host = data[0];
                                Int32.TryParse(args[2], out port);
                            }
                            else return;

                            StartClient();
                            break;
                        }
                    case string s when (s == "create" || s == "room"):
                        {
                            if (args.Length == 2)
                            {
                                Int32.TryParse(args[2], out port);
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
                                user.SetColor(ColorParser.GetColorFromString(args[1]));

                                if (isConnected)
                                {
                                    Message update = new Message(7, user);  //Updates userData on server
                                    SendMessage(update);
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
