using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class NetworkManager
    {
        public User User;
        public readonly Cmd Cmd;

        private Server server;
        private TcpClient client;

        private Thread listenThread;
        private Thread receiveThread;

        private string id = "null";

        protected internal string Host = "127.0.0.1";
        protected internal int Port = 23;

        private bool isConnected = false;
        private bool isServer = false;
        private bool receiveMessage = false;

        private NetworkStream stream;
        private readonly Action notification;

        public NetworkManager(Action notification)
        {
            Cmd = new Cmd();
            this.notification = notification;
        }

        public string Process()
        {
            return Cmd.ReadLine(User);      //Always read command and parse it in main thread
        }

        protected internal void RegisterUser()
        {
            tryOnceMore:
            Console.Write("Enter your name: ");
            var userName = Console.ReadLine();
            if(userName != null && userName.Length > 16)
            {
                Console.WriteLine("Name is too long");
                goto tryOnceMore;
            }

            Console.Title = userName!;             //Set title for user with him userName

            Console.Write("Enter your color (white): ");
            var color = Console.ReadLine();

            Console.Clear();

            User = new User(userName, ColorParser.GetColorFromString(color));   //Parse color from string and create user
        }

        public bool StartClient()
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
                client = new TcpClient(Host, Port);
                isConnected = true;
                stream = client.GetStream();            //Connects to the server and gets its stream

                var userDataMessage = new Message(8, User);      //Sends UserData to the server
                stream.Write(userDataMessage.Serialize());

                Cmd.WriteLine("Successfully connected to the server");

                GetId();                                //Gets own ID

                receiveMessage = true;

                receiveThread = new Thread(ReceiveMessage); //Starting receive message thread
                receiveThread.Start();
                return true;
            }
            catch (Exception e)
            {
                Cmd.WriteLine("Can't connect to the server: " + e.Message);
                return false;
            }
        }

        public void UpdateUserData()
        {
            if (!isConnected) return;
            var update = new Message(7, User);  //Updates userData on server
            SendMessage(update);
        }

        public bool StartServer()
        {
            if(server != null)                  //If server was already started
            {
                server.Disconnect();            //Then close it
                server = null;

                listenThread?.Interrupt();
            }

            try
            {
                server = new Server(Cmd, Port, notification); //Start new server
                isServer = true;

                if (isConnected) isConnected = false;   //If it was client. No, its doesnt)

                listenThread = new Thread(server.Listen);
                listenThread.Start();

                isServer = true;

                return true;
            }
            catch(Exception e)
            {
                Cmd.WriteLine("Can't start server: " + e.Message);
                return false;
            }
        }

        private void ReceiveMessage()
        {
            while(receiveMessage)
            {
                try
                {
                    var message = GetMessage();

                    if (message == null || message.Length <= 0) continue;
                    var msg = new Message(message);     //Lets get it
                    ParseMessage(msg);                      //And parse
                }
                catch (ThreadInterruptedException) { return; }
                catch (System.IO.IOException) { return; }       //If Thread was interrupted or something
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
                var data = new byte[64];
                var builder = new StringBuilder();

                if (stream == null) return null;
                do
                {
                    stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data));
                } while (stream.DataAvailable);         //Lets read this while stream is available

                return Encoding.Unicode.GetBytes(builder.ToString());   //Return message bytes array

            }
            catch (NullReferenceException) { return null; }

            catch (System.IO.IOException)                       //If stream was stopped
            { 
                Cmd.WriteLine("You are disconnected"); 
                Cmd.SwitchToPrompt();
                if (isConnected) StopClient();
                else if (isServer) DisconnectServer();
                return null;
            }

            catch (Exception e)                                 //If something went wrong
            {
                Cmd.WriteLine("Can't get message from thread: " + e.Message);

                return null;
            }
        }

        public Input GetInputType(string input)
        {
            if (input.Trim().Length < 1) return Input.Empty;
            return input[0] == '/' ? Input.Command : Input.Message;
        }

        public string[] GetCommandArgs(string input)
        {
            if (GetInputType(input) == Input.Command)
            {
                var lower = input.ToLower();
                var args = lower.Split(" ");
                args[0] = args[0].Substring(1);

                return args;
            }

            return new string[0];
        }

        public bool IsConnectedToServer()
        {
            return (client != null || stream != null) && isConnected;
        }

        public bool TryCreateRoom(string port)
        {
            int.TryParse(port, out Port);

            return StartServer();
        }
        public bool TryJoin(params string[] joinCommand)
        {
            switch (joinCommand.Length)
            {
                case 2:
                {
                    var data = joinCommand[1].Split(":");

                    int.TryParse(data[1], out Port);
                    Host = data[0];
                    break;
                }
                case 3:
                    Host = joinCommand[0];
                    int.TryParse(joinCommand[1], out Port);
                    break;
                default:
                    return false;
            }

            return StartClient();
        }
        public void TryDisconnect()
        {
            if (isConnected)
                DisconnectClient();

            else if (isServer) 
                DisconnectServer();
        }

        /// <summary>
        /// Sends messages with PostCodes between 1-4
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="postCode"></param>
        private void SendMessage(string msg, int postCode)
        {
            try
            {
                if (msg.Trim().Length < 0) throw new Exception("Message is empty"); //If message empty
                var message = new Message(postCode, User, msg);
                var data = message.Serialize();

                Cmd.UserWriteLine(msg, User);
                stream.Write(data, 0, data.Length);         //Serialize message and send it
            }
            catch(Exception e)
            {
                Cmd.WriteLine("Can't send message: " + e.Message);  //If something went wrong
            }
        }
        private void SendMessage(Message msg)
        {
            try
            {
                var data = msg.Serialize();

                if(data.Length > 0)             //If message is not empty
                {
                    stream.Write(data, 0, data.Length); //Send it
                }
            }
            catch(Exception e)
            {
                Cmd.WriteLine("Can't send message: " + e.Message);  //If something went wrong ))
            }
        }
        public void SendMessage(string msg)
        {
            if (isConnected)
                SendMessage(msg, 1);

            else if (isServer) 
                SendServerMessage(msg, 1);

            else 
                Cmd.UserWriteLine(msg, User);
        }

        private void SendServerMessage(string msg, int postCode)
        {
            try
            {
                if (msg.Trim().Length < 1) throw new Exception("Message is empty");  //The same situation
                var message = new Message(postCode, User, msg);

                Cmd.UserWriteLine(msg, User);
                server.BroadcastFromServer(message);                            //Broadcast message to all clients
            }
            catch(Exception e)
            {
                Cmd.WriteLine("Can't send message from the server: " + e.Message);
            }
        }

        private void GetId()
        {
            var msg = new Message(GetMessage());

            id = msg.TextMessage;
            Cmd.WriteLine("Your ID: "+ id);
        }

        private void StopClient()
        {
            if (receiveMessage)
            {
                receiveMessage = false;
                receiveThread.Interrupt();
                receiveThread = null;
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

        private void DisconnectClient()
        {
            Message msg = new Message(9, User);  //Send message to server about disconnecting
            stream.Write(msg.Serialize());       //Disconnect this client from server

            StopClient();
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
                    Cmd.UserWriteLine(message.TextMessage, message.Sender); notification(); break;
                case 8:
                    Cmd.ConnectionMessage(message.Sender, "has joined"); break;
                case 9:
                    Cmd.ConnectionMessage(message.Sender, "has disconnected"); break;
                case 10:
                    {
                        DisconnectClient();                 //If server sends us message about stopping
                        Cmd.WriteLine("Server was stopped"); //We are decide to write this
                        break;
                    }
                default: return;
            }

            Cmd.SwitchToPrompt();                           //Lets go back to console
        }
    }
}
