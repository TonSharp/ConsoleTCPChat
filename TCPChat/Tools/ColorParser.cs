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
                Color consoleColor = Color.White;
                if(color[0] != '#')
                {
                    KnownColor knownColor = Enum.Parse<KnownColor>(color, true);
                    consoleColor = Color.FromKnownColor(knownColor);

                    if (consoleColor == Color.Black) consoleColor = Color.White;
                }
                else if(color.Length == 7)
                {
                    consoleColor = System.Drawing.ColorTranslator.FromHtml(color);
                }

                return consoleColor;
            }
            catch
            {
                return Color.White;
            }
        }
    }
}
