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
        /// Use this only for sending id from server
        /// </summary>
        /// <param name="id">client id from the server</param>
        public Message(string id)
        {
            this.PostCode = 5;
            this.message = id;
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
            byte[] userData;
            byte[] messageData;
            byte[] Data;

            switch(PostCode)
            {
                case int i when (i >= 1 && i <= 4):
                    {
                        userData = Sender.Serialize();
                        messageData = Serializer.SerializeString(message);

                        Data = new byte[sizeof(int) + userData.Length + messageData.Length];

                        using (MemoryStream stream = new MemoryStream(Data))
                        {
                            BinaryWriter writer = new BinaryWriter(stream);
                            writer.Write(PostCode);
                            writer.Write(userData);
                            writer.Write(messageData);
                        }

                        return Data;
                    }

                case 5:
                    {
                        messageData = Serializer.SerializeString(message);

                        Data = new byte[sizeof(int) + messageData.Length];

                        using (MemoryStream stream = new MemoryStream(Data))
                        {
                            BinaryWriter writer = new BinaryWriter(stream);
                            writer.Write(PostCode);
                            writer.Write(messageData);
                        }

                        return Data;
                    }

                case int i when (i >= 8 && i <= 9):
                    {
                        userData = Sender.Serialize();

                        Data = new byte[sizeof(int) + userData.Length];

                        using (MemoryStream stream = new MemoryStream(Data))
                        {
                            BinaryWriter writer = new BinaryWriter(stream);
                            writer.Write(PostCode);
                            writer.Write(userData);
                        }

                        return Data;
                    }

                default: return null;
            }
        }
        public void Deserialize(byte[] data)
        {
            byte[] userData;
            byte[] messageData;

            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);
                PostCode = reader.ReadInt32();

                switch(PostCode)
                {
                    case int i when (i >= 1 && i <= 4):
                        {
                            userData = Serializer.CopyFrom(data, sizeof(int));
                            Sender = new User(userData, out messageData);

                            message = Serializer.DeserializeString(messageData, 1)[0];
                            break;
                        }

                    case 5:
                        {
                            messageData = Serializer.CopyFrom(data, sizeof(int));

                            message = Serializer.DeserializeString(messageData, 1)[0];
                            break;
                        }

                    case int i when (i >= 8 && i <= 9):
                        {
                            userData = Serializer.CopyFrom(data, sizeof(int));
                            Sender = new User(userData);

                            break;
                        }
                }
            }
        }
    }
}

//POST CODES:
//1 - Send default message
//2 - Send warning message
//3 - Send error message
//4 - Send notification message
//5 - Get ID message
//6 - Get UserData message
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