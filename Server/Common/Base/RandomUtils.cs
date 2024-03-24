using System;

namespace Common
{
    public static class RandomUtils
    {
        private static Random m_Random = new Random();
        
        public static void SetSeed(int seed)
        {
            m_Random = new Random(seed);
        }
        
        public static int Range(int min, int max)
        {
            return m_Random.Next(min, max);
        }
        
        public static float Range(float min, float max)
        {
            return (float)m_Random.NextDouble() * (max - min) + min;
        }
    }
}