using System;
using System.Collections.Generic;
using System.Text;

namespace TCPChat
{
    public class CMD
    {
        private Vector2 messagePosition, promptPosition;

        private void CheckBufferArea()
        {
            if (messagePosition.Y >= promptPosition.Y)
            {
                Console.MoveBufferArea(0, 1, Console.BufferWidth, messagePosition.Y, 0, 0);
                messagePosition.Y--;
            }
        }

        public CMD()
        {
            messagePosition = new Vector2(0,0);
            promptPosition = new Vector2(0, 10);
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

        public void JoiningMessage(User Sender)
        {
            Console.ForegroundColor = Sender.Color;
            Console.SetCursorPosition(messagePosition.X, messagePosition.Y);
            Console.Write(Sender.UserName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" has joined");
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
            Console.SetCursorPosition(2, promptPosition.Y);
        }

        public void ParseMessage(Message message)
        {
            switch(message.PostCode)
            {
                case 1:
                    {
                        UserWriteLine(message.message, message.Sender);
                        break;
                    }
                case 8:
                    {
                        JoiningMessage(message.Sender);
                        messagePosition.Y++;

                        break;
                    }

                case 9:
                    {
                        Console.ForegroundColor = message.Sender.Color;
                        Console.SetCursorPosition(messagePosition.X, messagePosition.Y);
                        Console.WriteLine(message.Sender.UserName + " has disconnected");
                        messagePosition.Y++;

                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            SwitchToPrompt();
        }
    }
}
