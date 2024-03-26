using System;

namespace Common
{
    public struct Vector3
    {
        private const float Epsilon = 0.0001f;
        
        public float x, y, z;
        public override string ToString()
        {
            return $"[{x}, {y}, {z}]";
        }
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public float Magnitude()
        {
            return (float)Math.Sqrt(SqrMagnitude());
        }

        public float SqrMagnitude()
        {
            return x * x + y * y + z * z;
        }
        
        public Vector3 Normalize()
        {
            var mag = Magnitude();
            if (Math.Abs(mag) < Epsilon)
            {
                return new Vector3(0, 0, 0);
            }
            return new Vector3(x / mag, y / mag, z / mag);
        }

        public bool IsZero()
        {
            return Math.Abs(x) < Epsilon && Math.Abs(y) < Epsilon && Math.Abs(z) < Epsilon;
        }
        
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 operator *(Vector3 a, float scalar)
        {
            return new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
        }
    }
}