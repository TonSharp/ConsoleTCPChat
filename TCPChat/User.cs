using System;
using System.Collections.Generic;
using System.Text;

namespace TCPChat
{
    public class User
    {
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
    }
}
