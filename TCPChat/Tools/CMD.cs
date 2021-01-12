using System;
using System.Collections.Generic;
using System.Text;

namespace TCPChat
{
    public class CMD
    {
        private Vector2 messagePosition, promptPosition, defaultPromptPos;

        private void CheckBufferArea()
        {
            if (messagePosition.Y >= promptPosition.Y)
            {
                Console.SetCursorPosition(promptPosition.X, promptPosition.Y);
                Console.Write("  ");
                promptPosition.Y++;
                Console.SetCursorPosition(promptPosition.X, promptPosition.Y);
            }
        }

        public CMD()
        {
            messagePosition = new Vector2(0,0);
            defaultPromptPos = new Vector2(0, 15);
            promptPosition = new Vector2(0, defaultPromptPos.Y);
            messagePosition.PositionChanged += CheckBufferArea;
        }

        public void Write<T>(T message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorTop = messagePosition.Y;
            Console.WriteLine(message);
            messagePosition.Y++;
        }

        public void Write<T>(T message, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.CursorTop = messagePosition.Y;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            messagePosition.Y++;
        }

        public void WriteLine<T>(T message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(messagePosition.X, messagePosition.Y);
            Console.WriteLine(message);
            messagePosition.Y++;
        }

        public void WriteLine<T>(T message, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.SetCursorPosition(messagePosition.X, messagePosition.Y);
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            messagePosition.Y++;
        }

        public void UserWriteLine<T>(T message, User Sender)
        {
            Console.ForegroundColor = Sender.Color;
            Console.SetCursorPosition(messagePosition.X, messagePosition.Y);
            Console.Write(Sender.UserName + ": ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            messagePosition.Y++;
        }

        public void ConnectionMessage(User Sender, string str)
        {
            Console.ForegroundColor = Sender.Color;
            Console.SetCursorPosition(messagePosition.X, messagePosition.Y);
            Console.Write(Sender.UserName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" " + str);
            messagePosition.Y++;
        }

        public string ReadLine()
        {
            Console.SetCursorPosition(promptPosition.X, promptPosition.Y);
            Console.Write("> ");
            string input = Console.ReadLine();
            Console.SetCursorPosition(promptPosition.X, promptPosition.Y);
            Console.Write(new String(' ', input.Length + 2));

            return input;
        }

        public void SwitchToPrompt()
        {
            Console.SetCursorPosition(0, promptPosition.Y);
            Console.Write("> ");
        }

        public void ParseMessage(Message message)
        {
            switch(message.PostCode)
            {
                case 1: 
                    UserWriteLine(message.message, message.Sender); break;
                case 8: 
                    ConnectionMessage(message.Sender, "has joined"); break;
                case 9: 
                    ConnectionMessage(message.Sender, "has disconnected"); break;
                default: return;
            }

            CheckBufferArea();
            SwitchToPrompt();
        }

        public void Clear()
        {
            Console.Clear();
            messagePosition.Y = 0;
            promptPosition.Y = defaultPromptPos.Y;
        }
    }
}
