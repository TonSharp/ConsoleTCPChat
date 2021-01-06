using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPChat
{
    class Program
    {
        static Server server;
        static Thread thread;

        static string userName;
        private static string host = "127.0.0.1";
        private static int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        static void StartServer()
        {
            try
            {
                server = new Server();
                thread = new Thread(new ThreadStart(server.Listen));
                thread.Start();

                userName = "Server";
                Console.WriteLine("Welcome, ", userName);
                Thread sendthread = new Thread(new ThreadStart(SendFromServer));
                sendthread.Start();
            }
            catch(Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        static void StartClient()
        {
            Console.Write("Enter your name: ");
            userName = Console.ReadLine();
            client = new TcpClient();

            try
            {
                client.Connect(host, port);
                stream = client.GetStream();

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();

                Console.WriteLine("Welcome, {0}", userName);
                SendMessage();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        static void Main(string[] args)
        {
        askAgain:
            Console.WriteLine("Server or client? (s/c)");
            string option = Console.ReadLine();
            option = option.ToLower();

            if (option[0] == 's' || option == "server")
                StartServer();

            else if (option[0] == 'c' || option == "client")
                StartClient();

            else goto askAgain;
        }

        static void SendFromServer()
        {
            Console.WriteLine("Введите сообщение: ");

            while(true)
            {
                string message = "Server: " + Console.ReadLine();
                server.BroadcastFromServer(message);
            }
        }

        static void SendMessage()
        {
            Console.WriteLine("Введите сообщение: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
    }
}

