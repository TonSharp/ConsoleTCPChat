using System;

namespace TCPChat.AudioEngine
{
    public static class Sound
    {
        public static CachedSound TryLoadCached(string path)
        {
            CachedSound sound;

            try
            {
                sound = new CachedSound(path);
            }
            catch
            {
                sound = null;
            }

            return sound;
        }
    }
}