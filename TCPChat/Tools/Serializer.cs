using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TCPChat
{
    public static class Serializer
    {
        /// <summary>
        /// Copy part of StartData starting and specified index
        /// </summary>
        /// <param name="StartData">Origin</param>
        /// <param name="MoveFrom">Starting index copy from</param>
        /// <returns>New copied byte array</returns>
        public static byte[] CopyFrom(byte[] StartData, int MoveFrom)
        {
            byte[] MovedData = new byte[StartData.Length - MoveFrom];

            for (int i = 0; i < StartData.Length; i++)
            {
                if (i < MoveFrom) continue;

                else
                {
                    if (i - MoveFrom < MovedData.Length)
                    {
                        MovedData[i - MoveFrom] = StartData[i];
                    }
                }
            }

            return MovedData;
        }

        /// <summary>
        /// Joining two bytes array into one
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>Merged data1 + data2</returns>
        public static byte[] JoinBytes(byte[] data1, byte[] data2)
        {
            byte[] ExpandedData = new byte[data1.Length + data2.Length];

            data1.CopyTo(ExpandedData, 0);
            data2.CopyTo(ExpandedData, data1.Length);

            return ExpandedData;
        }

        /// <summary>
        /// Get string data size based on its size (sizeof(int) = 4) + string length * sizeof(char)
        /// </summary>
        /// <param name="str">string or array of strings</param>
        /// <returns>Serialized strings size</returns>
        public static int GetStringDataSize(params string[] str)
        {
            int size = 0;

            foreach (string s in str)
            {
                size += sizeof(int);
                size += sizeof(char) * s.Length;
            }

            return size;
        }

        /// <summary>
        /// Serialaizes string or string array and reurns byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Serialized string or strings array</returns>
        public static byte[] SerializeString(params string[] str)
        {
            byte[] data = new byte[GetStringDataSize(str)];

            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                foreach (string s in str)
                {
                    writer.Write(s.Length);
                    writer.Write(Encoding.Unicode.GetBytes(s));
                }
            }

            return data;
        }

        /// <summary>
        /// Deserialized string (!!!Important!!! The size of the data must not exceed the size of the rows)
        /// </summary>
        /// <param name="data">Serialized string data</param>
        /// <returns>Deserialized string or string array</returns>
        public static string[] DeserializeString(byte[] data)
        {
            List<string> deserialized = new List<string>();
            int bytes = 0;

            using (MemoryStream stream = new MemoryStream(data))
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

        /// <summary>
        /// Deserialize strings based on ist count
        /// </summary>
        /// <param name="data">Seriailized data</param>
        /// <param name="count">Number of serialized strings</param>
        /// <returns>Deserialized string or string array</returns>
        public static string[] DeserializeString(byte[] data, int count)
        {
            string[] deserialized = new string[count];

            using (MemoryStream stream = new MemoryStream(data))
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

        /// <summary>
        /// Deserialize strings based on its count and retruns string or string array
        /// </summary>
        /// <param name="data">Origin data</param>
        /// <param name="count">Count of strings</param>
        /// <param name="OtherData">Remainings data</param>
        /// <returns>Deserialized string or string array</returns>
        public static string[] DeserializeString(byte[] data, int count, out byte[] OtherData)
        {
            string[] deserialized = new string[count];

            using (MemoryStream stream = new MemoryStream(data))
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
