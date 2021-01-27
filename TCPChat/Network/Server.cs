using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TCPChat.Messages;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class Server
    {
        private static TcpListener _tcpListener;
        private readonly List<Client> clients = new List<Client>();
        private readonly Cmd localCmd;
        private readonly int port;
        public readonly Action Notification;

        public Server(Cmd cmd, int port, Action notification)
        {
            localCmd = cmd;
            this.port = port;
            this.Notification = notification;
        }

        protected internal void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            var client = clients.FirstOrDefault(c => c.Id == id);    //Pick client with specified ID

            if (client != null)                     //If its exist
                clients.Remove(client);             //Remove connection
        }

        protected internal void Listen()
        {
            try
            {
                    _tcpListener = new TcpListener(IPAddress.Any, port);
                    _tcpListener.Start();
                    localCmd.WriteLine("Server started, waiting for connections...");
                    localCmd.SwitchToPrompt();

                while (true)
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();        //If we get a new connection
                    var clientObject = new Client(tcpClient, this);          //Lets create new client

                    var clientThread = new Thread(clientObject.Process);    //And start new thread
                    clientThread.Start();
                }
            }
            catch (SocketException) {
            }
            catch (Exception ex)
            {
                localCmd.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(Message msg, string id)
        {
            var data = msg.Serialize();
            localCmd.ParseMessage(msg);                 //If we want to broadcast message, lets write it in the server

            foreach (var t in clients.Where(t => t.Id != id))
            {
                t.Stream.Write(data, 0, data.Length);  //And then if it isn a sender, send rhis message to client
            }
        }

        protected internal void BroadcastFromServer(Message msg)
        {
            var data = msg.Serialize();
            foreach (var t in clients)
            {
                t.Stream.Write(data, 0, data.Length);  //Send this message for all clients
            }
        }

        protected internal void Disconnect()
        {
            _tcpListener.Stop();
            localCmd.WriteLine("Server was stopped");

            foreach (var t in clients)
            {
                var msg = new PostCodeMessage(10);
                t.Stream.Write(msg.Serialize());
                t.Close();                         //And then close connection
            }
        }
    }
}