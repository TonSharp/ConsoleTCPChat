using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TCPChat
{
    public class Message
    {
        public int PostCode
        {
            get;
            private set;
        }
        public byte[] Data
        {
            get;
            private set;
        }
        public byte[] UserData
        {
            get;
            private set;
        }

        public User Sender
        {
            get;
            private set;
        }
        public string message
        {
            get;
            private set;
        }

        /// <summary>
        /// Use this for sending messages
        /// </summary>
        /// <param name="PostCode">Usual between 1 - 4 (for messages), 10 - 13 (for commands)</param>
        /// <param name="Sender">User who sends this</param>
        /// <param name="Message">Message or Command</param>
        public Message(int PostCode, User Sender, string Message)
        {
            this.PostCode = PostCode;
            this.Sender = Sender;


            if (PostCode != 8 || PostCode != 9)  //If Sender doesn't connect or disconnect
                message = Message;
        }

        /// <summary>
        /// Use this only for Connect or Disconnect
        /// </summary>
        /// <param name="PostCode">8 or 9</param>
        /// <param name="Sender">Who connected/disconnected?</param>
        public Message(int PostCode, User Sender)
        {
            if (PostCode == 8 || PostCode == 9)
            {
                this.PostCode = PostCode;
                this.Sender = Sender;
            }
        }

        /// <summary>
        /// Use this only for Deserialization
        /// </summary>
        public Message(byte[] data)
        {
            Deserialize(data);
        }

        public byte[] Serialize()
        {
            int dataSize;
            byte[] data;

            UserData = UserSerialize(Sender);

            if (PostCode == 8 || PostCode == 9)
            {
                dataSize = sizeof(int) + UserData.Length;
                data = new byte[dataSize];

                using (var stream = new MemoryStream(data))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(PostCode);
                    writer.Write(UserData);
                }

                return data;
            }
            else
            {
                Data = StringSerialize(message);

                dataSize = sizeof(int) + UserData.Length + Data.Length;
                data = new byte[dataSize];

                using (var stream = new MemoryStream(data))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(PostCode);
                    writer.Write(UserData);
                    writer.Write(Data);
                }

                return data;
            }
        }
        public void Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);
                PostCode = reader.ReadInt32();

                if(PostCode == 8 || PostCode == 9)
                {
                    int NameLength = reader.ReadInt32();
                    string name = Encoding.Unicode.GetString(reader.ReadBytes(sizeof(char) * NameLength));
                    int ColorLength = reader.ReadInt32();
                    string color = Encoding.Unicode.GetString(reader.ReadBytes(sizeof(char) * ColorLength));

                    Sender = new User(name, ColorParser.GetColorFromString(color));
                }
                else
                {
                    int NameLength = reader.ReadInt32();
                    string name = Encoding.Unicode.GetString(reader.ReadBytes(sizeof(char) * NameLength));
                    int ColorLength = reader.ReadInt32();
                    string color = Encoding.Unicode.GetString(reader.ReadBytes(sizeof(char) * ColorLength));
                    int MessageLength = reader.ReadInt32();
                    message = Encoding.Unicode.GetString(reader.ReadBytes(sizeof(char) * MessageLength));

                    Sender = new User(name, ColorParser.GetColorFromString(color));
                }
            }
        }

        private int SizeOfStringData(params string[] str)
        {
            int size = 0;

            foreach(var s in str)
            {
                size += sizeof(int);
                size += sizeof(char) * s.Length;
            }

            return size;
        }

        private byte[] StringSerialize(string str)
        {
            int dataSize = SizeOfStringData(str);

            byte[] data = new byte[dataSize];

            using (var stream = new MemoryStream(data))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(str.Length);
                writer.Write(Encoding.Unicode.GetBytes(str));
            }

            return data;
        }

        private string StringDeserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);
                int Length = reader.ReadInt32();
                string str = Encoding.Unicode.GetString(reader.ReadBytes(sizeof(char)*Length));

                return str;
            }
        }

        private byte[] UserSerialize(User Sender)
        {
            int userDataSize = SizeOfStringData(Sender.UserName, Sender.Color.ToString());
            byte[] userData = new byte[userDataSize];

            using (var stream = new MemoryStream(userData))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(StringSerialize(Sender.UserName));
                writer.Write(StringSerialize(Sender.Color.ToString()));
                //writer.Write(Sender.Color.ToString());
            }

            return userData;
        }
    }
}

//POST CODES:
//1 - Send default message
//2 - Send warning message
//3 - Send error message
//4 - Send notification message
//5 - reserved
//6 - reserved
//7 - reserved
//8 - Join message
//9 - Disconnect message
//10 - Send user command
//11 - Send admin command
//12 - Send server command
//13 - reserved
//14 - reserved
//15 - reserved
//16 - reserved
//17 - reserved
//18 - reserved
//19 - reserved