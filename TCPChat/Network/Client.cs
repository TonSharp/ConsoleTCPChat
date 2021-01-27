using System;
using System.Net.Sockets;
using System.Text;
using TCPChat.Messages;
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

                while (true)
                {
                    try
                    {
                        var message = GetMessage();  //While stream is available lets read stream
                        if (message.Length > 0)         //If message is not empty
                        {
                            var msg = IMessageDeserializable.Parse(message);

                            switch(msg.PostCode)
                            {
                                case { } i when (i >= 1 && i <= 4):
                                {
                                    server.Notification();
                                    server.BroadcastMessage(msg, Id);   //If this is regular message then broadcast it
                                    break;
                                }
                                case 6:                                         //if client updates his UserData
                                {
                                    var userDataMessage = msg as UserDataMessage;
                                    if(userDataMessage?.Method == Method.Send)
                                        user = new User(userDataMessage?.Sender.UserName, userDataMessage.Sender.Color); //Update UserData on server
                                    
                                    break;
                                }
                                case 7:
                                {
                                    var idMessage = msg as IDMessage;
                                    if (idMessage?.Method == Method.Get)
                                    {
                                        var sendMessage = new IDMessage(Method.Send, Id);
                                        Stream.Write(sendMessage.Serialize());
                                    }

                                    break;
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
                        var disconnectionMsg = new ConnectionMessage(Connection.Disconnect, user);          //If there is error, disconnect this user
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
            var messageData = GetMessage();
            var msg = new ConnectionMessage(messageData);

            if (!VersionVerifier.Verify(msg.Hash))
            {
                Stream.Write(new PostCodeMessage(11).Serialize());
                server.RemoveConnection(Id);

                return;
            }

            SendId();
            server.BroadcastMessage(msg, Id);
        }

        private void SendId()
        {
            var msg = new IDMessage(Method.Send, Id);
            Stream.Write(msg.Serialize());
        }

        private byte[] GetMessage()
        {
            var data = new byte[64];
            var builder = new StringBuilder();
            do
            {
                var bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return Encoding.Unicode.GetBytes(builder.ToString());
        }

        protected internal void Close()
        {
            Stream?.Close();
            client?.Close();
        }
    }
}