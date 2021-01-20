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
            Id = Guid.NewGuid().ToString();                     //Generate new unigue ID
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {  
            try
            {
                Message msg;
                Stream = client.GetStream();    //Gets stream
                    
                InitializeUserData();           //Gets userData
                SendID();                       //Sends ID, Oddly enough)

                while (true)
                {
                    try
                    {
                        string message = GetMessage();  //While stream is available lets read stream
                        if (message.Length > 0)         //If message is not empty
                        {
                            msg = new Message(Encoding.Unicode.GetBytes(message));  //Lets parse it

                            switch(msg.PostCode)
                            {
                                case int i when (i >= 1 && i <= 4):
                                    {
                                        server.Notification();
                                        server.BroadcastMessage(msg, Id);   //If this is regular message then broadcast it
                                        break;
                                    }
                                case 7:                                         //if client updates his UserData
                                    {
                                        user = new User(msg.Sender.UserName, msg.Sender.Color); //Update UserData on server
                                        continue;
                                    }
                                case 9:                                     //If user Disconnecting
                                    {
                                        server.BroadcastMessage(msg, Id);   //Broadcast it
                                        server.RemoveConnection(Id);        //And remove connection
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
                        Message disconnMsg = new Message(9, user);          //If there is error, disconnect this user
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