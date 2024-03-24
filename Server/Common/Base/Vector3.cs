namespace Common
{
    public struct Vector3
    {
        public float x, y, z;
        public override string ToString()
        {
            return $"[{x}, {y}, {z}]";
        }
    }
}