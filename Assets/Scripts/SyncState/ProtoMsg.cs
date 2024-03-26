using System.IO;
using UnityEngine;

namespace SyncState
{
    internal class InputMsg
    {
        public Vector3 Dir;

        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(Dir.x);
            writer.Write(Dir.y);
            writer.Write(Dir.z);
            return stream.ToArray();
        }

        public void Unserialize(byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            Dir.x = reader.ReadSingle();
            Dir.y = reader.ReadSingle();
            Dir.z = reader.ReadSingle();
        }
    }

    internal class StateMsg
    {
        public string ClientKey;
        public Vector3 TargetPosition;
        public Vector3 TargetForward;
        public float TimeStamp;

        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(ClientKey);
            writer.Write(TargetPosition.x);
            writer.Write(TargetPosition.y);
            writer.Write(TargetPosition.z);
            writer.Write(TargetForward.x);
            writer.Write(TargetForward.y);
            writer.Write(TargetForward.z);
            writer.Write(TimeStamp);
            return stream.ToArray();
        }

        public void Unserialize(byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            ClientKey = reader.ReadString();
            TargetPosition.x = reader.ReadSingle();
            TargetPosition.y = reader.ReadSingle();
            TargetPosition.z = reader.ReadSingle();
            TargetForward.x = reader.ReadSingle();
            TargetForward.y = reader.ReadSingle();
            TargetForward.z = reader.ReadSingle();
            TimeStamp = reader.ReadSingle();
        }
    }
}
