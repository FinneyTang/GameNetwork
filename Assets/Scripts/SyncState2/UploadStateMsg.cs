using System.IO;
using UnityEngine;

class UploadStateMsg
{
    public Vector3 TargetPosition;
    public Quaternion TargetOrientation;
    public float TimeStamp;
    public byte[] Serialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(TargetPosition.x);
        writer.Write(TargetPosition.y);
        writer.Write(TargetPosition.z);
        writer.Write(TargetOrientation.x);
        writer.Write(TargetOrientation.y);
        writer.Write(TargetOrientation.z);
        writer.Write(TargetOrientation.w);
        writer.Write(TimeStamp);
        return stream.ToArray();
    }
    public void Unserialize(byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        TargetPosition.x = reader.ReadSingle();
        TargetPosition.y = reader.ReadSingle();
        TargetPosition.z = reader.ReadSingle();
        TargetOrientation.x = reader.ReadSingle();
        TargetOrientation.y = reader.ReadSingle();
        TargetOrientation.z = reader.ReadSingle();
        TargetOrientation.w = reader.ReadSingle();
        TimeStamp = reader.ReadSingle();
    }
}