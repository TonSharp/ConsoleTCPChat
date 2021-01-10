using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TCPChat
{
    //TODO:
    //1. Drop here serialization/deserialization
    public class User
    {
        private int userDataSize
        {
            get => Serializer.GetStringDataSize(UserName, Color.ToString());        
        }

        public string UserName
        {
            get;
            private set;
        }
        public ConsoleColor Color
        {
            get;
            private set;
        }



        public User(string Name, ConsoleColor Color)
        {
            UserName = Name;
            this.Color = Color;
        }

        /// <summary>
        /// Use this only for deserialization
        /// </summary>
        /// <param name="Data">Data that begins from UserData</param>
        /// <param name="OtherData">Data that lefts after UserData, such as message, command or file data</param>
        public User(byte[] Data, out byte[] OtherData)
        {
            string[] userDataStrings = Serializer.DeserializeString(Data, 2, out OtherData);

            UserName = userDataStrings[0];
            Color = ColorParser.GetColorFromString(userDataStrings[1]);
        }

        public void Deserialize(byte[] Data, out byte[] OtherData)
        {
            byte[] userData = new byte[userDataSize];
            byte[] otherData;

            OtherData = Serializer.CopyFrom(Data, userDataSize);

            string[] userDataStrings = Serializer.DeserializeString(userData, 2, out otherData);

            UserName = userDataStrings[0];
            Color = ColorParser.GetColorFromString(userDataStrings[1]);
        }

        public byte[] Serialize()
        {
            byte[] data = new byte[userDataSize];

            using(var stream = new MemoryStream(data))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(Serializer.SerializeString(UserName, Color.ToString()));
            }

            return data;
        }
    }
}
