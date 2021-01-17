using System;
using System.Drawing;
using Console = Colorful.Console;

namespace TCPChat
{
    public class CMD
    {
        private readonly Vector2 messagePos, promptPos, defaultPromptPos, currentPromptPos;

        /// <summary>
        /// Checks buffer, if message top position close to prompt position then move prompt
        /// </summary>
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

        /// <summary>
        /// Writes message in the same line
        /// </summary>
        /// <typeparam name="T">Type of message (string, int, e.t.c.)</typeparam>
        /// <param name="message">Message</param>
        public void Write<T>(T message)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.CursorTop = messagePos.Y;
            Console.WriteLine(message, Color.White);
            messagePos.Y++;
        }

        /// <summary>
        /// Writes message in the same line with specified color
        /// </summary>
        /// <typeparam name="T">Type of message (string, int, e.t.c.)</typeparam>
        /// <param name="message">Message</param>
        public void Write<T>(T message, Color Color)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.CursorTop = messagePos.Y;
            Console.WriteLine(message, Color);
            messagePos.Y++;
        }

        /// <summary>
        /// Writes message in begining of line
        /// </summary>
        /// <typeparam name="T">Type of message (string, int, e.t.c.)</typeparam>
        /// <param name="message">Message</param>
        public void WriteLine<T>(T message)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.WriteLine(message, Color.White);
            messagePos.Y++;
        }

        /// <summary>
        /// Writes message in begining of line with specified color
        /// </summary>
        /// <typeparam name="T">Type of message (string, int, e.t.c.)</typeparam>
        /// <param name="message">Message</param>
        public void WriteLine<T>(T message, Color Color)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.WriteLine(message, Color);
            Console.ForegroundColor = Color.White;
            messagePos.Y++;
        }

        /// <summary>
        /// Writes message from user
        /// </summary>
        /// <typeparam name="T">Type of message (string, int, e.t.c.)</typeparam>
        /// <param name="message">Message</param>
        /// <param name="Sender">Who sends this message</param>
        public void UserWriteLine<T>(T message, User Sender)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.Write(Sender.UserName, Sender.Color);
            Console.Write(": ", Color.White);
            Console.WriteLine(message, Color.White);
            messagePos.Y++;
        }

        /// <summary>
        /// Write message about connection
        /// </summary>
        /// <param name="Sender">Who connects, disconnects</param>
        /// <param name="str">Connection message</param>
        public void ConnectionMessage(User Sender, string str)
        {
            currentPromptPos.X = Console.CursorLeft;
            currentPromptPos.Y = promptPos.Y;

            Console.SetCursorPosition(messagePos.X, messagePos.Y);
            Console.Write(Sender.UserName, Sender.Color);
            Console.WriteLine(" " + str, Color.White);
            messagePos.Y++;
        }

        /// <summary>
        /// Reads commands in command prompt
        /// </summary>
        /// <param name="user">Who enter commands</param>
        /// <returns>Input string</returns>
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

        /// <summary>
        /// Switches back to command prompt in old position afetr message recieving
        /// </summary>
        public void SwitchToPrompt()
        {
            Console.SetCursorPosition(currentPromptPos.X, currentPromptPos.Y);
        }

        /// <summary>
        /// Parses messages PostCodes
        /// </summary>
        /// <param name="message">Recieved Message</param>
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

        /// <summary>
        /// Clears console buffer
        /// </summary>
        public void Clear()
        {
            Console.Clear();
            messagePos.Y = 0;
            promptPos.Y = defaultPromptPos.Y;
        }
    }
}
