using TCPChat.Network;

namespace TCPChat.Messages
{
    public abstract class Message : IMessage
    {
        public int PostCode { get; protected set; }
        public User Sender { get; protected set; }
        public string SendData { get; protected set; }
        public Method Method { get; protected set; }

        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] data);
    }
}

//POST CODES:
//1 - Send default message +
//2 - Send warning message +-
//3 - Send error message +-
//4 - Send notification message +-
//5 - Get ID message +
//6 - Get UserData message -
//7 - Send UserData message 
//8 - Join message
//9 - Disconnect message
//10 - Server Disconnect message
//11 - Send ID from server message +
//12 - Send UserData from server message
//13 - Send Unsupported version message TODO:
//14 - reserved
//15 - reserved
//16 - reserved
//17 - reserved
//18 - reserved
//19 - reserved