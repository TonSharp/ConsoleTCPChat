using System;
using System.Net.Sockets;
using System.Threading;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class Connector
    {
        public Server Server { get; private set; }
        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public ConnectionType ConnectionType { get; private set; }


        public Connector(ConnectionType connectionConnectionType = ConnectionType.None, string host = "127.0.0.1", int port = 23)
        {
            ConnectionType = connectionConnectionType;
            Host = host;
            Port = port;
        }

        public void StartServer(Cmd cmd, Action notification)
        {
            if (Server != null)
            {
                Server.Disconnect();
                Server = null;
            }

            try
            {
                Server = new Server(cmd, Port, notification);
                ConnectionType = ConnectionType.Server;
            }
            catch
            {
                Server = null;
            }
        }

        public void StartClient(Thread receiveThread, Thread listenThread)
        {
            if (Stream != null || Client != null)
            {
                StopClient(receiveThread);
            }

            if (ConnectionType == ConnectionType.Server)
            {
                StopServer(listenThread);
            }

            try
            {
                Client = new TcpClient(Host, Port);
                ConnectionType = ConnectionType.Client;
                Stream = Client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void StopClient(Thread receiveThread)
        {
            try
            {
                if (Stream != null)
                {
                    Stream.Close();
                    Stream.Dispose();
                    Stream = null;
                }

                if (Client != null)
                {
                    Client.Close();
                    Client.Dispose();
                    Client = null;
                }

                ConnectionType = ConnectionType.None;
                receiveThread.Interrupt();
            }
            catch
            {
                // ignored
            }
        }

        private void StopServer(Thread listenThread)
        {
            try
            {
                Server.Disconnect();
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