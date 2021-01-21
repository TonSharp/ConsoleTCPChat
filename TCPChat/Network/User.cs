using System;
using System.Drawing;
using System.IO;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class User
    {
        private int UserDataSize
        {
            get
            {
                var size = Serializer.GetStringDataSize(UserName);
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

        public User(string name, Color color)
        {
            UserName = name;
            Color = color;
        }

        /// <summary>
        /// Use this only for deserialization. Creates user based on Serialized UserData
        /// </summary>
        /// <param name="data">Data that begins from UserData</param>
        /// <param name="otherData">Remaining data after UserData</param>
        public User(byte[] data, out byte[] otherData)                 //Rewrite without deserialize
        {
            UserName = Serializer.DeserializeString(data, 1)[0];        //Deserialize UserName
            var userDataSize = Serializer.GetStringDataSize(UserName);
            var colorData = Serializer.CopyFrom(data, userDataSize);

            using (var stream = new MemoryStream(colorData))
            {
                var reader = new BinaryReader(stream);
                Color = Color.FromArgb(reader.ReadInt32());
                userDataSize += sizeof(int);
            }

            otherData = Serializer.CopyFrom(data, userDataSize);        
        }

        /// <summary>
        /// Use this only for deserialization. Creates user based on Serialized UserData
        /// </summary>
        /// <param name="data">Data that begins from UserData</param>
        public User(byte[] data)                                       //Rewrite without deserialize
        {
            UserName = Serializer.DeserializeString(data, 1)[0];
            var nameDataSize = Serializer.GetStringDataSize(UserName);

            var colorData = Serializer.CopyFrom(data, nameDataSize);

            using var stream = new MemoryStream(colorData);
            var reader = new BinaryReader(stream);

            Color = Color.FromArgb(reader.ReadInt32());
        }

        /// <summary>
        /// Deserialize data based on usedDataSize
        /// </summary>
        /// <param name="data">Data for deserialization</param>
        /// <param name="otherData">Remaining data after UserData</param>
        public void Deserialize(byte[] data, out byte[] otherData)
        {
            var userData = new byte[UserDataSize];

            otherData = Serializer.CopyFrom(data, UserDataSize);

            var userDataStrings = Serializer.DeserializeString(userData, 1, out var colorData);

            UserName = userDataStrings[0];
            Color = Color.FromArgb(Convert.ToInt32(colorData));
        }

        /// <summary>
        /// Serialize UserData
        /// </summary>
        /// <returns>Serialized byte array</returns>
        public byte[] Serialize()
        {
            var data = new byte[UserDataSize];

            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);

            writer.Write(Serializer.SerializeString(UserName));
            writer.Write(Color.ToArgb());

            return data;
        }
    }
}
