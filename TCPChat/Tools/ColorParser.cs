using System;

namespace TCPChat
{
    public static class ColorParser
    {
        public static ConsoleColor GetColorFromString(string color)
        {
            ConsoleColor consoleColor;
            Enum.TryParse(color, true, out consoleColor);
            if (consoleColor == ConsoleColor.Black) consoleColor = ConsoleColor.White;

            return consoleColor;
        }
    }
}
