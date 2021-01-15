using System;
using System.Drawing;

namespace TCPChat
{
    public static class ColorParser
    {
        public static Color GetColorFromString(string color)
        {
            Color consoleColor = Color.FromName(color);
            if (consoleColor == Color.Black) consoleColor = Color.White;

            return consoleColor;
        }
    }
}
