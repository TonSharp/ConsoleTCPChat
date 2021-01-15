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



        public User(string Name, Color Color)
        {
            UserName = Name;
            this.Color = Color;
        }

        /// <summary>
        /// Use this only for deserialization
        /// </summary>
        /// <param name="Data">Data that begins from UserData</param>
        /// <param name="OtherData">Data that lefts after UserData, such as message, command or file data</param>
        public User(byte[] Data, out byte[] OtherData)                 //Rewrite without deserialize
        {
            Deserialize(Data, out OtherData);
        }

        /// <summary>
        /// Use this only for deserialization
        /// </summary>
        /// <param name="Data">Data that begins from UserData</param>
        public User(byte[] Data)                                       //Rewrite without deserialize
        {
            Deserialize(Data, out byte[] empty);
        }

        public void Deserialize(byte[] Data, out byte[] OtherData)
        {
            byte[] userData = new byte[userDataSize];
            byte[] colorData;

            OtherData = Serializer.CopyFrom(Data, userDataSize);

            string[] userDataStrings = Serializer.DeserializeString(userData, 1, out colorData);

            UserName = userDataStrings[0];
            Color = Color.FromArgb(Convert.ToInt32(colorData));
        }

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
