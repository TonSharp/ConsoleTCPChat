using System;
using System.Drawing;

namespace TCPChat.Tools
{
    public static class ColorParser
    {
        public static Color GetColorFromString(string color)
        {
            try
            {
                var consoleColor = Color.White;
                if(color[0] != '#')         //If this is not html color
                {
                    KnownColor knownColor = Enum.Parse<KnownColor>(color, true);
                    consoleColor = Color.FromKnownColor(knownColor);    //Just parse it
                }
                else if(color.Length == 7)
                {
                    consoleColor = ColorTranslator.FromHtml(color); // Or parse html color
                    byte[] byteColor = BitConverter.GetBytes(consoleColor.ToArgb());

                    if(byteColor[0] <= 48 && byteColor[1] <= 48 && byteColor[2] <= 48)
                    {
                        consoleColor = Color.White;
                    }
                }


                if (consoleColor == Color.Black) consoleColor = Color.White; //If this is Black Color, then it will be White color, no racism)

                return consoleColor;
            }

            catch
            {
                return Color.White;
            }
        }
    }
}
