using TCPChat.Network;
using System.IO;
using TCPChat.Tools;

namespace TCPChat.Messages
{
    public class SimpleMessage : Message
    {
        public SimpleMessage(User sender, string message)
        {
            this.Sender = sender;
            PostCode = 1;
            SendData = message;
            Method = Method.Send;
        }
        
        public override byte[] Serialize()
        {
            var userData = Sender.Serialize();
            var messageData = Serializer.SerializeString(SendData);
            var data = new byte[sizeof(int) + userData.Length + messageData.Length];

            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);
            writer.Write(PostCode);
            writer.Write(userData);
            writer.Write(messageData);

            return data;
        }

        public override void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);

            var code = reader.ReadInt32();
            if (code != 1) return;
            PostCode = code;
            
            var userData = Serializer.CopyFrom(data, sizeof(int));
            Sender = new User(userData, out var messageData);

            SendData = Serializer.DeserializeString(messageData, 1)[0];
        }
    }
}