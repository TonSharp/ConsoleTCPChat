using System;
using System.Collections.Generic;
using System.Text;

namespace TCPChat
{
    public static class ColorParser
    {
        public static ConsoleColor GetColorFromString(string color)
        {
            color = color.ToLower();

            return color switch
            {
                "red"   => ConsoleColor.Red,
                "green" => ConsoleColor.Green,
                "blue"  => ConsoleColor.Blue,
                "white" => ConsoleColor.White,
                _       => ConsoleColor.White,
            };
        }
    }
}
