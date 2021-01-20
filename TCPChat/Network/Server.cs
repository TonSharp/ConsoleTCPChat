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
        public Action Notification;

        public Server(CMD cmd, int port, Action Notification)
        {
            LocalCMD = cmd;
            this.port = port;
            this.Notification = Notification;
        }

        protected internal void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            Client client = clients.FirstOrDefault(c => c.Id == id);    //Pick client with specified ID

            if (client != null)                     //If its exist
                clients.Remove(client);             //Remove connection
        }

        protected internal void Listen()
        {
            try
            {
                    tcpListener = new TcpListener(IPAddress.Any, port);
                    tcpListener.Start();
                    LocalCMD.WriteLine("Server started, waiting for connections...");
                    LocalCMD.SwitchToPrompt();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();        //If we get a new connection
                    Client clientObject = new Client(tcpClient, this);          //Lets create new client

                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));    //And start new thread
                    clientThread.Start();
                }
            }
            catch (System.Net.Sockets.SocketException) { return; }
            catch (Exception ex)
            {
                LocalCMD.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(Message msg, string id)
        {
            byte[] data = msg.Serialize();
            LocalCMD.ParseMessage(msg);                 //If we want to broadcast message, lets write it in the server

            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id)
                    clients[i].Stream.Write(data, 0, data.Length);  //And then if it isn a sender, send rhis message to client
            }
        }

        protected internal void BroadcastFromServer(Message msg)
        {
            byte[] data = msg.Serialize();
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Stream.Write(data, 0, data.Length);  //Send this message for all clients
            }
        }

        protected internal void Disconnect()
        {
            Message msg;

            tcpListener.Stop();
            LocalCMD.WriteLine("Server was stoped");

            for (int i = 0; i < clients.Count; i++)
            {
                msg = new Message(10);                      //Send closing message to clients
                clients[i].Stream.Write(msg.Serialize());
                clients[i].Close();                         //And then close connection
            }
        }
    }
}