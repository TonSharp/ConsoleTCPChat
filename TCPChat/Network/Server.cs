using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace TCPChat
{
    public class Server
    {
        static TcpListener tcpListener;
        List<Client> clients = new List<Client>();
        CMD LocalCMD;

        public Server(CMD cmd)
        {
            LocalCMD = cmd;
        }

        public void Write(Message msg)
        {
            LocalCMD.UserWriteLine(msg.message, msg.Sender);
        }

        protected internal void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            Client client = clients.FirstOrDefault(c => c.Id == id);

            if (client != null)
                clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                LocalCMD.WriteLine("Server started, waiting for connections...");
                LocalCMD.SwitchToPrompt();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    Client clientObject = new Client(tcpClient, this);

                    NetworkStream stream = tcpClient.GetStream();

                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    User user = GetUser(Encoding.Unicode.GetBytes(builder.ToString()));
                    clientObject.user = user;

                    BroadcastMessage(new Message(8, user), clientObject.Id);

                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        private User GetUser(byte[] data)
        {
            Message message = new Message(data);

            return message.Sender;
        }

        protected internal void BroadcastMessage(Message msg, string id)
        {
            byte[] data = msg.Serialize();
            LocalCMD.ParseMessage(msg);
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].Id != id)
                    clients[i].Stream.Write(data, 0, data.Length);
            }
        }

        protected internal void BroadcastFromServer(Message msg)
        {
            byte[] data = msg.Serialize();
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Stream.Write(data, 0, data.Length);
            }
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}