using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPChat
{
    class Program
    {
        static User user;
        static CMD cmd;
        static Server server;
        static Thread ListenThread;
        static Thread SendThread;

        private static string host = "127.0.0.1";
        private static int port = 23;
        static TcpClient client;
        static NetworkStream stream;

        static void RegisterUser()
        {
            Console.Write("Enter your name: ");
            string userName = Console.ReadLine();
            Console.Write("Enter your color (white): ");
            string color = Console.ReadLine();
            Console.Clear();

            user = new User(userName, ColorParser.GetColorFromString(color));
        }

        static void ReadHost()
        {
            Console.Write("Enter host (127.0.0.1): ");
            string temphost = Console.ReadLine();

            if (temphost.Length > 1) host = temphost;

            Console.Clear();
        }

        static void StartServer()
        {
            try
            {
                RegisterUser();

                server = new Server(cmd);
                ListenThread = new Thread(new ThreadStart(server.Listen));
                ListenThread.Start();

                Console.Write("Welcome, ");
                cmd.Write(user.UserName, user.Color);

                SendThread = new Thread(new ThreadStart(SendFromServer));
                SendThread.Start();
            }
            catch(Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        static void StartClient()
        {
            RegisterUser();
            ReadHost();

            client = new TcpClient();

            try
            {
                client.Connect(host, port);
                stream = client.GetStream();

                Message message = new Message(8, user);

                byte[] data = message.Serialize();
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();

                Console.Write("Welcome, ");
                cmd.Write(user.UserName, user.Color);
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
            cmd = new CMD();

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
            while(true)
            {
                string message = cmd.ReadLine();
                Message msg = new Message(1, user, message);
                cmd.UserWriteLine(message, user);
                server.BroadcastFromServer(msg);
            }
        }

        static void SendMessage()
        {
            while (true)
            {
                string message = cmd.ReadLine();
                cmd.UserWriteLine(message, user);
                Message msg = new Message(1, user, message);

                byte[] data = msg.Serialize();
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

                    if(builder.ToString().Length > 0)
                    {
                        Message msg = new Message(Encoding.Unicode.GetBytes(builder.ToString()));
                        cmd.ParseMessage(msg);
                    }
                }
                catch(Exception ex)
                {
                    cmd.WriteLine("Lost Connection: " + ex.Message);
                    cmd.ReadLine();
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

