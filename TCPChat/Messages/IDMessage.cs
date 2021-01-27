using System;
using System.IO;
using TCPChat.Tools;

namespace TCPChat.Messages
{
    // ReSharper disable once InconsistentNaming
    public class IDMessage : Message
    {

        public IDMessage(Method method, string id = "")
        {
            Method = method;
            PostCode = 5;

            if (Method == Method.Send)
            {
                SendData = id;
                PostCode = 11;
            }
        }
        
        public override byte[] Serialize()
        {
            var methodData = Serializer.SerializeString(Method.ToString());
            byte[] data = null;
            
            switch (Method)
            {
                case Method.Get:
                {
                    data = new byte[sizeof(int) + methodData.Length];
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);
                    
                    writer.Write(PostCode);
                    writer.Write(methodData);

                    break;
                }
                    
                case Method.Send:
                {
                    var idData = Serializer.SerializeString(SendData);
                    data = new byte[sizeof(int) + methodData.Length + idData.Length];
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);
                    
                    writer.Write(PostCode);
                    writer.Write(methodData);
                    writer.Write(idData);

                    break;
                }
            }

            return data;
        }

        public override void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var code = reader.ReadInt32();

            if (code != 5 && code != 11) return;
            PostCode = code;

            switch (PostCode)
            {
                case 5:
                {
                    Method = Enum.Parse<Method>(
                        Serializer.DeserializeString(Serializer.CopyFrom(data, sizeof(int)), 1)[0]
                    );

                    break;
                }

                case 11:
                {
                    string[] messageArgs = Serializer.DeserializeString(Serializer.CopyFrom(data, sizeof(int)), 2);

                    Method = Enum.Parse<Method>(messageArgs[0]);
                    SendData = messageArgs[1];

                    break;
                }
            }
        }
    }
}