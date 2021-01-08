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
        TcpClient client;
        Server server;

        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Message msg;
                Stream = client.GetStream();

                while (true)
                {
                    try
                    {
                        string message = GetMessage();
                        if(message.Length > 1)
                        {
                            msg = new Message(Encoding.Unicode.GetBytes(message));
                            if(msg.PostCode != 8 && msg.PostCode != 9 && msg.message != "")
                            {
                                server.BroadcastMessage(msg, this.Id);
                            }
                        }
                    }
                    catch
                    {
                        //server.BroadcastMessage(msg, this.Id);
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
                server.RemoveConnection(this.Id);
                Close();
            }
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