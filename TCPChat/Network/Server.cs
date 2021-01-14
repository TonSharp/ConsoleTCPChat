using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPChat
{
    public class Server
    {
        private static TcpListener tcpListener;
        private readonly List<Client> clients = new List<Client>();
        private readonly CMD LocalCMD;
        private readonly int port;

        private bool isListen = true;

        public Server(CMD cmd, int port)
        {
            LocalCMD = cmd;
            this.port = port;
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
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                LocalCMD.WriteLine("Server started, waiting for connections...");
                LocalCMD.SwitchToPrompt();

                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Client clientObject = new Client(tcpClient, this);

                Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                clientThread.Start();
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
            if (msg.PostCode == 9) RemoveConnection(id);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id)
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

        //TODO:
        //1. Send message about closing
        protected internal void Disconnect()
        {
            Message msg;
            tcpListener.Stop();
            LocalCMD.WriteLine("Server was stoped");

            for (int i = 0; i < clients.Count; i++)
            {
                msg = new Message(10);                      //Closing message
                clients[i].Stream.Write(msg.Serialize());
                clients[i].Close();
            }
        }
    }
}