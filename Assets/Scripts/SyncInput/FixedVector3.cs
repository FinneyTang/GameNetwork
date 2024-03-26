using UnityEngine;

public class FixedVector3
{
    public static readonly FixedVector3 zero = new FixedVector3(0, 0, 0);
    
    public int x, y, z;
    
    public FixedVector3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x / 1000f, y / 1000f, z / 1000f);
    }
    
    public static FixedVector3 operator *(FixedVector3 v, int scalar)
    {
        return new FixedVector3(
            (int)((long)v.x * scalar / 1000), 
            (int)((long)v.y * scalar / 1000), 
            (int)((long)v.z * scalar / 1000));
    }
    
    public static FixedVector3 operator +(FixedVector3 a, FixedVector3 b)
    {
        return new FixedVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    
    public override string ToString()
    {
        return $"[{x}, {y}, {z}]";
    }
}