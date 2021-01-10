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
            byte[] userData = Sender.Serialize();

            byte[] data = new byte[sizeof(int) + userData.Length];

            using (var stream = new MemoryStream(data))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(PostCode);
                writer.Write(userData);
            }

            if(PostCode != 8 && PostCode != 9)
            {
                byte[] Data = Serializer.SerializeString(message);
                byte[] ExpandedData = Serializer.JoinBytes(data, Data);

                return ExpandedData;
            }

            return data;
        }
        public void Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);
                PostCode = reader.ReadInt32();

                byte[] userData = Serializer.CopyFrom(data, sizeof(int));
                byte[] otherData;

                Sender = new User(userData, out otherData);

                if(PostCode != 8 && PostCode != 9)
                {
                    message = Serializer.DeserializeString(otherData, 1)[0];
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