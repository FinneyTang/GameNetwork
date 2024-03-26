using System.Collections.Generic;
using System.IO;

namespace SyncInput
{
    internal class InputMsg
    {
        public int X;
        public int Y;
        
        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(X);
            writer.Write(Y);
            return stream.ToArray();
        }
        
        public void Unserialize(byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
        }
    }

    internal class FrameClientInputsMsg
    {
        public class ClientInputData
        {
            public string ClientKey;
            public int X;
            public int Y;
        }

        public int FrameCount;
        public readonly List<ClientInputData> ClientInputs = new List<ClientInputData>();

        public byte[] Serialize()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(FrameCount);
            writer.Write(ClientInputs.Count);
            foreach (var clientInput in ClientInputs)
            {
                writer.Write(clientInput.ClientKey);
                writer.Write(clientInput.X);
                writer.Write(clientInput.Y);
            }  
            return stream.ToArray();
        }
        
        public void Unserialize(byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            FrameCount = reader.ReadInt32();
            var clientInputsLength = reader.ReadInt32();
            for (var i = 0; i < clientInputsLength; i++)
            {
                ClientInputs.Add(new ClientInputData
                {
                    ClientKey = reader.ReadString(),
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32()
                });
            }
        }
    }
}