using System;
using System.Net.Sockets;
using System.Text;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class Client
    {
        protected internal string Id { get; }
        protected internal NetworkStream Stream { get; private set; }
        private User user;
        private readonly TcpClient client;
        private readonly Server server;

        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();                     //Generate new unique ID
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {  
            try
            {
                Stream = client.GetStream();    //Gets stream
                    
                InitializeUserData();           //Gets userData
                SendId();                       //Sends ID, Oddly enough)

                while (true)
                {
                    try
                    {
                        var message = GetMessage();  //While stream is available lets read stream
                        if (message.Length > 0)         //If message is not empty
                        {
                            var msg = new Message(Encoding.Unicode.GetBytes(message));

                            switch(msg.PostCode)
                            {
                                case { } i when (i >= 1 && i <= 4):
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
                        var disconnectionMsg = new Message(9, user);          //If there is error, disconnect this user
                        server.BroadcastMessage(disconnectionMsg, Id);
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

        private void InitializeUserData()
        {
            var messageData = Encoding.Unicode.GetBytes(GetMessage());

            if (messageData[0] != 8 && messageData[0] != 9) return;
            var userData = Serializer.CopyFrom(messageData, 4);
            user = new User(userData);

            server.BroadcastMessage(new Message(8, user), Id);
        }

        private void SendId()
        {
            var msg = new Message(11, Id);
            Stream.Write(msg.Serialize());
        }

        private string GetMessage()
        {
            var data = new byte[64];
            var builder = new StringBuilder();
            do
            {
                var bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            Stream?.Close();
            client?.Close();
        }
    }
}