using System;
using System.Drawing;
using Console = Colorful.Console;

namespace TCPChat
{
    public class CMD
    {
        private readonly Vector2 messagePos, promptPos, defaultPromptPos, currentPromptPos;

        private void CheckBufferArea()
        {
            if (messagePos.Y >= promptPos.Y)
            {
                Console.SetCursorPosition(promptPos.X, promptPos.Y);
                Console.Write("  ");
                promptPos.Y++;
                Console.SetCursorPosition(promptPos.X, promptPos.Y);
            }
        }

        public CMD()
        {
            messagePos = new Vector2(0, 0);
            defaultPromptPos = new Vector2(0, 15);
            currentPromptPos = new Vector2(0, defaultPromptPos.Y);
            promptPos = new Vector2(0, defaultPromptPos.Y);
            messagePos.PositionChanged += CheckBufferArea;
        }

        public void Write<T>(T message)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.ForegroundColor = Color.White;
            Console.CursorTop = messagePos.Y;
            Console.WriteLine(message);
            messagePos.Y++;
        }

        public void Write<T>(T message, Color Color)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.ForegroundColor = Color;
            Console.CursorTop = messagePos.Y;
            Console.WriteLine(message);
            Console.ForegroundColor = Color.White;
            messagePos.Y++;
        }

        public void WriteLine<T>(T message)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.ForegroundColor = Color.White;
            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.WriteLine(message);
            messagePos.Y++;
        }

        public void WriteLine<T>(T message, Color Color)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.ForegroundColor = Color;
            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.WriteLine(message);
            Console.ForegroundColor = Color.White;
            messagePos.Y++;
        }

        public void UserWriteLine<T>(T message, User Sender)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.ForegroundColor = Sender.Color;
            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.Write(Sender.UserName + ": ");
            Console.ForegroundColor = Color.White;
            Console.WriteLine(message);
            messagePos.Y++;
        }

        public void ConnectionMessage(User Sender, string str)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.ForegroundColor = Sender.Color;
            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.Write(Sender.UserName);
            Console.ForegroundColor = Color.White;
            Console.WriteLine(" " + str);
            messagePos.Y++;
        }

        public string ReadLine(User user)
        {
            Console.SetCursorPosition(promptPos.X, promptPos.Y);
            Console.Write(user.UserName, user.Color);
            Console.Write("> ", Color.White);
            string input = Console.ReadLine();
            Console.SetCursorPosition(promptPos.X, promptPos.Y);
            Console.Write(new string(' ',user.UserName.Length + input.Length + 2));

            return input;
        }

        public void SwitchToPrompt()
        {
            Console.SetCursorPosition(currentPromptPos.X, currentPromptPos.Y);
        }

        public void ParseMessage(Message message)
        {
            switch (message.PostCode)
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
            messagePos.Y = 0;
            promptPos.Y = defaultPromptPos.Y;
        }
    }
}
