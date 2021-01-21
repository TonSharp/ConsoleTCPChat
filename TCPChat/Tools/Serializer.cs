using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TCPChat.Tools
{
    public static class Serializer
    {
        /// <summary>
        /// Copy part of StartData starting and specified index
        /// </summary>
        /// <param name="startData">Origin</param>
        /// <param name="moveFrom">Starting index copy from</param>
        /// <returns>New copied byte array</returns>
        public static byte[] CopyFrom(byte[] startData, int moveFrom)
        {
            var movedData = new byte[startData.Length - moveFrom];

            for (int i = 0; i < startData.Length; i++)
            {
                if (i < moveFrom) continue;

                if (i - moveFrom < movedData.Length)
                {
                    movedData[i - moveFrom] = startData[i];
                }
            }

            return movedData;
        }

        /// <summary>
        /// Joining two bytes array into one
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>Merged data1 + data2</returns>
        public static byte[] JoinBytes(byte[] data1, byte[] data2)
        {
            byte[] expandedData = new byte[data1.Length + data2.Length];

            data1.CopyTo(expandedData, 0);
            data2.CopyTo(expandedData, data1.Length);

            return expandedData;
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
        /// Serializes string or string array and returns byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Serialized string or strings array</returns>
        public static byte[] SerializeString(params string[] str)
        {
            var data = new byte[GetStringDataSize(str)];

            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);

            foreach (var s in str)
            {
                writer.Write(s.Length);
                writer.Write(Encoding.Unicode.GetBytes(s));
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
        /// <param name="data">Serialized data</param>
        /// <param name="count">Number of serialized strings</param>
        /// <returns>Deserialized string or string array</returns>
        public static string[] DeserializeString(byte[] data, int count)
        {
            var deserialized = new string[count];

            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream);

                for (var i = 0; i < count; i++)
                {
                    var size = reader.ReadInt32();
                    deserialized[i] = Encoding.Unicode.GetString(reader.ReadBytes(size * sizeof(char)));
                }
            }

            return deserialized;
        }

        /// <summary>
        /// Deserialize strings based on its count and returns string or string array
        /// </summary>
        /// <param name="data">Origin data</param>
        /// <param name="count">Count of strings</param>
        /// <param name="otherData">Remaining data</param>
        /// <returns>Deserialized string or string array</returns>
        public static string[] DeserializeString(byte[] data, int count, out byte[] otherData)
        {
            string[] deserialized = new string[count];

            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream);

                for (var i = 0; i < count; i++)
                {
                    var size = reader.ReadInt32();
                    deserialized[i] = Encoding.Unicode.GetString(reader.ReadBytes(size * sizeof(char)));
                }
            }

            int stringDataSize = GetStringDataSize(deserialized);

            otherData = CopyFrom(data, stringDataSize);

            return deserialized;
        }
    }
}
