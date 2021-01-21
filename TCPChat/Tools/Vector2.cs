using System;

namespace TCPChat.Tools
{
    public class Vector2
    {
        private int x, y;

        public int X
        {
            get => x;
            set
            {
                x = Convert.ToInt32(value);
                PositionChanged?.Invoke();
            }
        }
        public int Y
        {
            get => y;
            set
            {
                y = Convert.ToInt32(value);
                PositionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Function that will be called after position will be changed
        /// </summary>
        public event Action PositionChanged;

        public Vector2(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(Action onPositionChanged, int x = 0, int y = 0) : this(x, y)
        {
            PositionChanged = onPositionChanged;
        }
    }
}
