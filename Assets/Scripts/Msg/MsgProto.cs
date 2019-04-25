using System.IO;
using UnityEngine;

class MsgProto
{
    public abstract class MsgBase
    {
        public abstract byte[] Serialize();
        public abstract void Unserialize(byte[] data);
    }
    public class MoveToMsg : MsgBase
    {
        public int PlayerID;
        public Vector3 TargetPosition;
        public float Speed;

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(PlayerID);
            writer.Write(TargetPosition.x);
            writer.Write(TargetPosition.y);
            writer.Write(TargetPosition.z);
            writer.Write(Speed);
            return stream.ToArray();
        }
        public override void Unserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            PlayerID = reader.ReadInt32();
            TargetPosition.x = reader.ReadSingle();
            TargetPosition.y = reader.ReadSingle();
            TargetPosition.z = reader.ReadSingle();
            Speed = reader.ReadSingle();
        }
    }
    private static readonly byte[] KEY = new byte[] { 0x36, 0x7F, 0x45 };
    public static byte[] XOR(byte[] data)
    {
        int keyIndex = 0;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= KEY[keyIndex];
            keyIndex = (keyIndex + 1) % KEY.Length;
        }
        return data;
    }
}
