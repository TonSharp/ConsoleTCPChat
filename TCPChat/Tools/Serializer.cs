using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TCPChat
{
    public static class Serializer
    {
        public static byte[] CopyFrom(byte[] StartData, int MoveFrom)
        {
            byte[] MovedData = new byte[StartData.Length - MoveFrom];

            for(int i = 0; i < StartData.Length; i++)
            {
                if (i < MoveFrom) continue;

                else
                {
                    if(i - MoveFrom < MovedData.Length)
                    {
                        MovedData[i - MoveFrom] = StartData[i];
                    }
                }
            }

            return MovedData;
        }

        public static byte[] JoinBytes(byte[] data1, byte[] data2)
        {
            byte[] ExpandedData = new byte[data1.Length + data2.Length];

            data1.CopyTo(ExpandedData, 0);
            data2.CopyTo(ExpandedData, data1.Length);

            return ExpandedData;
        }

        public static int GetStringDataSize(params string[] str)
        {
            int size = 0;

            foreach (var s in str)
            {
                size += sizeof(int);
                size += sizeof(char) * s.Length;
            }

            return size;
        }

        public static byte[] SerializeString(params string[] str)
        {
            byte[] data = new byte[GetStringDataSize(str)];

            using (var stream = new MemoryStream(data))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                foreach (var s in str)
                {
                    writer.Write(s.Length);
                    writer.Write(Encoding.Unicode.GetBytes(s));
                }
            }

            return data;
        }

        public static string[] DeserializeString(byte[] data)
        {
            List<string> deserialized = new List<string>();
            int bytes = 0;

            using (var stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);

                while (bytes < data.Length - 1)
                {
                    int size = reader.ReadInt32();
                    bytes += sizeof(int);
                    deserialized.Add(Encoding.Unicode.GetString(reader.ReadBytes(size)));
                    bytes += sizeof(char) * size;
                }
            }

            return deserialized.ToArray();
        }

        public static string[] DeserializeString(byte[] data, int count)
        {
            string[] deserialized = new string[count];

            using (var stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);

                for (int i = 0; i < count; i++)
                {
                    int size = reader.ReadInt32();
                    deserialized[i] = Encoding.Unicode.GetString(reader.ReadBytes(size * sizeof(char)));
                }
            }

            int stringDataSize = GetStringDataSize(deserialized);

            return deserialized;
        }

        public static string[] DeserializeString(byte[] data, int count, out byte[] OtherData)
        {
            string[] deserialized = new string[count];

            using (var stream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(stream);

                for (int i = 0; i < count; i++)
                {
                    int size = reader.ReadInt32();
                    deserialized[i] = Encoding.Unicode.GetString(reader.ReadBytes(size * sizeof(char)));
                }
            }

            int stringDataSize = GetStringDataSize(deserialized);

            OtherData = CopyFrom(data, stringDataSize);

            return deserialized;
        }
    }
}
