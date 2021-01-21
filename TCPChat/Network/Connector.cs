using System;
using System.Net.Sockets;
using System.Threading;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class Connector
    {
        public Server server { get; private set; }
        public TcpClient client { get; private set; }
        public NetworkStream stream { get; private set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public ConnectionType ConnectionType { get; set; }


        public Connector(ConnectionType connectionConnectionType = ConnectionType.None, string host = "127.0.0.1", int port = 23)
        {
            ConnectionType = connectionConnectionType;
            Host = host;
            Port = port;
        }

        public Server StartServer(Cmd cmd, Action notification)
        {
            if (server != null)
            {
                server.Disconnect();
                server = null;
            }

            try
            {
                server = new Server(cmd, Port, notification);
                ConnectionType = ConnectionType.Server;
            }
            catch
            {
                server = null;
            }

            return server;
        }

        public TcpClient StartClient(Thread receiveThread, Thread listenThread)
        {
            if (stream != null || client != null)
            {
                StopClient(receiveThread);
            }

            if (ConnectionType == ConnectionType.Server)
            {
                StopServer(listenThread);
            }

            try
            {
                client = new TcpClient(Host, Port);
                ConnectionType = ConnectionType.Client;
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return client;
        }

        public void StopClient(Thread receiveThread)
        {
            try
            {
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

                ConnectionType = ConnectionType.None;
                receiveThread.Interrupt();
            }
            catch
            {
                // ignored
            }
        }

        public void StopServer(Thread listenThread)
        {
            try
            {
                server.Disconnect();
                ConnectionType = ConnectionType.None;
                listenThread.Interrupt();
            }
            catch
            {
                // ignored
            }
        }

        public void Disconnect(Thread receiveThread, Thread listenThread)
        {
            switch (ConnectionType)
            {
                case ConnectionType.Client: StopClient(receiveThread); 
                    break;
                case ConnectionType.Server: StopServer(listenThread);
                    break;
            }
        }
    }
}