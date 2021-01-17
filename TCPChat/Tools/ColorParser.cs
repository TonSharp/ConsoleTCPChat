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
                if(color[0] != '#')         //If this is not html color
                {
                    KnownColor knownColor = Enum.Parse<KnownColor>(color, true);
                    consoleColor = Color.FromKnownColor(knownColor);    //Just parse it
                }
                else if(color.Length == 7)
                {
                    consoleColor = System.Drawing.ColorTranslator.FromHtml(color); // Or parse html color
                }


                if (consoleColor == Color.Black) consoleColor = Color.White; //If this is Black Color, then it will be White color, no rasizm)

                return consoleColor;
            }

            catch
            {
                return Color.White;
            }
        }
    }
}
