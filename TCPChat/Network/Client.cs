using System;
using System.Net.Sockets;
using System.Text;

namespace TCPChat
{
    public class Client
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public User user;
        private readonly TcpClient client;
        private readonly Server server;

        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {  
            //TODO:
            //1. Client should send information about update any userData
            try
            {
                Message msg;
                Stream = client.GetStream();

                InitializeUserData();
                SendID();

                while (true)
                {
                    try
                    {
                        string message = GetMessage();
                        if (message.Length > 0)
                        {
                            msg = new Message(Encoding.Unicode.GetBytes(message));

                            switch(msg.PostCode)
                            {
                                case int i when (i >= 1 && i <= 4):
                                    {
                                        server.BroadcastMessage(msg, Id);
                                        break;
                                    }
                                case 7:                                         //if client updates his UserData
                                    {
                                        user = new User(msg.Sender.UserName, msg.Sender.Color);
                                        continue;
                                    }
                                case 9:
                                    {
                                        server.BroadcastMessage(msg, Id);
                                        server.RemoveConnection(Id);
                                        break;
                                    }
                                default:
                                    {
                                        continue;
                                    }
                            }
                        }
                    }
                    catch
                    {
                        Message disconnMsg = new Message(9, user);
                        server.BroadcastMessage(disconnMsg, Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
                Close();
            }
        }

        public void InitializeUserData()
        {
            byte[] messageData = Encoding.Unicode.GetBytes(GetMessage());

            if (messageData[0] == 8 || messageData[0] == 9)
            {
                byte[] userData = Serializer.CopyFrom(messageData, 4);
                user = new User(userData);

                server.BroadcastMessage(new Message(8, user), Id);
            }
        }

        void SendID()
        {
            Message msg = new Message(11, Id);
            Stream.Write(msg.Serialize());
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}