using System.IO;
using UnityEngine;

class InputMsg
{
    public Vector2 Dir;
    public byte[] Serialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(Dir.x);
        writer.Write(Dir.y);
        return stream.ToArray();
    }
    public void Unserialize(byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        Dir.x = reader.ReadSingle();
        Dir.y = reader.ReadSingle();
    }
}