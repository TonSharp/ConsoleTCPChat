using System;
using System.Drawing;

namespace TCPChat
{
    public static class ColorParser
    {
        public static Color GetColorFromString(string color)
        {
            try
            {
                KnownColor knownColor = Enum.Parse<KnownColor>(color, true);
                Color consoleColor = Color.FromKnownColor(knownColor);
                if (consoleColor == Color.Black) consoleColor = Color.White;

                return consoleColor;
            }
            catch
            {
                return Color.White;
            }
        }
    }
}
