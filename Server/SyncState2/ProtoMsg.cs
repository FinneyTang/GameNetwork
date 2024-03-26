using System.IO;
using Common;

namespace SyncState2
{
    internal class UploadStateMsg
    {
        public Vector3 TargetPosition;
        public Vector3 TargetForward;
        public float TimeStamp;
        
        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
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
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            TargetPosition.x = reader.ReadSingle();
            TargetPosition.y = reader.ReadSingle();
            TargetPosition.z = reader.ReadSingle();
            TargetForward.x = reader.ReadSingle();
            TargetForward.y = reader.ReadSingle();
            TargetForward.z = reader.ReadSingle();
            TimeStamp = reader.ReadSingle();
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