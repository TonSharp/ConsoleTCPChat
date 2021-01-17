using System;
using System.Drawing;
using System.IO;

namespace TCPChat
{
    public class User
    {
        private int userDataSize
        {
            get
            {
                int size = Serializer.GetStringDataSize(UserName);
                size += sizeof(int); //ARGB Color

                return size;
            }
        }

        public string UserName
        {
            get;
            private set;
        }
        public Color Color
        {
            get;
            private set;
        }

        public void SetColor(Color color)
        {
            Color = color;
        }

        public User(string Name, Color Color)
        {
            UserName = Name;
            this.Color = Color;
        }

        /// <summary>
        /// Use this only for deserialization. Creates user based on Serialized UserData
        /// </summary>
        /// <param name="Data">Data that begins from UserData</param>
        /// <param name="OtherData">Remaining data afer UserData</param>
        public User(byte[] Data, out byte[] OtherData)                 //Rewrite without deserialize
        {
            int userDataSize;
            byte[] colorData;

            UserName = Serializer.DeserializeString(Data, 1)[0];        //Deserialize UserName
            userDataSize = Serializer.GetStringDataSize(UserName);
            colorData = Serializer.CopyFrom(Data, userDataSize);        //Copy colorData from Data

            using (MemoryStream stream = new MemoryStream(colorData))
            {
                BinaryReader reader = new BinaryReader(stream);
                Color = Color.FromArgb(reader.ReadInt32());
                userDataSize += sizeof(int);
            }

            OtherData = Serializer.CopyFrom(Data, userDataSize);        
        }

        /// <summary>
        /// Use this only for deserialization. Creates user based on Serialized UserData
        /// </summary>
        /// <param name="Data">Data that begins from UserData</param>
        public User(byte[] Data)                                       //Rewrite without deserialize
        {
            int nameDataSize;
            byte[] colorData;

            UserName = Serializer.DeserializeString(Data, 1)[0];
            nameDataSize = Serializer.GetStringDataSize(UserName);

            colorData = Serializer.CopyFrom(Data, nameDataSize);

            using(MemoryStream stream = new MemoryStream(colorData))
            {
                BinaryReader reader = new BinaryReader(stream);

                Color = Color.FromArgb(reader.ReadInt32());
            }
        }

        /// <summary>
        /// Deserialize data based on usedDataSize
        /// </summary>
        /// <param name="Data">Data for deserialization</param>
        /// <param name="OtherData">Remaining data after UserData</param>
        public void Deserialize(byte[] Data, out byte[] OtherData)
        {
            byte[] userData = new byte[userDataSize];
            byte[] colorData;

            OtherData = Serializer.CopyFrom(Data, userDataSize);

            string[] userDataStrings = Serializer.DeserializeString(userData, 1, out colorData);

            UserName = userDataStrings[0];
            Color = Color.FromArgb(Convert.ToInt32(colorData));
        }

        /// <summary>
        /// Serialize UserData
        /// </summary>
        /// <returns>Serialized byte array</returns>
        public byte[] Serialize()
        {
            byte[] data = new byte[userDataSize];

            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(Serializer.SerializeString(UserName));
                writer.Write(Color.ToArgb());
            }

            return data;
        }
    }
}
