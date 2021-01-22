using System.IO;
using TCPChat.Tools;

namespace TCPChat.Network
{
    //TODO:
    //1. Add hash version verify system
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
        public string TextMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Use this for sending messages
        /// </summary>
        /// <param name="postCode">Usual between 1 - 4 (for messages), 10 - 13 (for commands)</param>
        /// <param name="sender">User who sends this</param>
        /// <param name="message">Message or Command</param>
        public Message(int postCode, User sender, string message)
        {
            this.PostCode = postCode;
            this.Sender = sender;


            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (postCode != 8 || postCode != 9)  //If Sender doesn't connect or disconnect
                TextMessage = message;
        }

        /// <summary>
        /// Use this only for Connect or Disconnect or Updating UserData on Server
        /// </summary>
        /// <param name="postCode">8 or 9, 7</param>
        /// <param name="sender">Who connected/disconnected, updating</param>
        public Message(int postCode, User sender)
        {
            this.PostCode = postCode;
            this.Sender = sender;
        }

        /// <summary>
        /// Use this from getting or setting user parametres
        /// </summary>
        /// <param name="postCode"></param>
        /// <param name="textMessage"></param>
        public Message(int postCode, string textMessage)
        {
            this.PostCode = postCode;
            this.TextMessage = textMessage;
        }

        /// <summary>
        /// Use this only for sending events from server
        /// </summary>
        /// <param name="postCode"></param>
        public Message(int postCode)
        {
            this.PostCode = postCode;
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
            byte[] data;

            switch(PostCode)
            {
                case { } i when (i >= 1 && i <= 4):             //If we send message
                    {
                        userData = Sender.Serialize();
                        messageData = Serializer.SerializeString(TextMessage);

                        data = new byte[sizeof(int) + userData.Length + messageData.Length];    //Size of PostCode + userData + MessageData

                        using var stream = new MemoryStream(data);
                        var writer = new BinaryWriter(stream);
                        writer.Write(PostCode);
                        writer.Write(userData);
                        writer.Write(messageData);

                        return data;
                    }

                case { } i when (i == 5 || i == 11):                        //If we sends or request ID
                    {
                        messageData = Serializer.SerializeString(TextMessage);
                        data = new byte[sizeof(int) + messageData.Length];  //Size of PostCode + ID

                        using var stream = new MemoryStream(data);
                        var writer = new BinaryWriter(stream);
                        writer.Write(PostCode);
                        writer.Write(messageData);

                        return data;
                    }

                case { } i when (i >= 7 && i <= 9):                         //If Update userData or Joining/Disconnecting
                    {
                        userData = Sender.Serialize();

                        data = new byte[sizeof(int) + userData.Length];

                        using var stream = new MemoryStream(data);
                        var writer = new BinaryWriter(stream);
                        writer.Write(PostCode);
                        writer.Write(userData);

                        return data;
                    }

                case 10:                                                            //Server disconnect message
                    {
                        data = new byte[sizeof(int)];

                        using var stream = new MemoryStream(data);
                        var writer = new BinaryWriter(stream);
                        writer.Write(PostCode);

                        return data;
                    }

                default: return null;
            }
        }

        private void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);                 //Firstly lets read PostCode
            PostCode = reader.ReadInt32();

            byte[] userData;
            byte[] messageData;
            switch(PostCode)                                                //Then lets deserialize based on PostCode
            {
                case { } i when (i >= 1 && i <= 4):                         //If its usual message
                {
                    userData = Serializer.CopyFrom(data, sizeof(int));
                    Sender = new User(userData, out messageData);

                    TextMessage = Serializer.DeserializeString(messageData, 1)[0];
                    break;
                }

                case { } i when (i == 5 || i == 11):                        //If we sends or request ID
                {
                    messageData = Serializer.CopyFrom(data, sizeof(int));

                    TextMessage = Serializer.DeserializeString(messageData, 1)[0];
                    break;
                }

                case { } i when (i >= 7 && i <= 9):                         //If we updating UserData or Joining/Disconnecting
                {
                    userData = Serializer.CopyFrom(data, sizeof(int));
                    Sender = new User(userData);

                    break;
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
//7 - Send userData message
//8 - Join message
//9 - Disconnect message
//10 - Server Disconnect message
//11 - Send ID from server message
//12 - Send UserData from server message
//13 - Send Unsupported version message TODO:
//14 - reserved
//15 - reserved
//16 - reserved
//17 - reserved
//18 - reserved
//19 - reserved