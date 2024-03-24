using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;

namespace CustomMsg
{
    internal class MsgProto
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
            var keyIndex = 0;
            for (var i = 0; i < data.Length; i++)
            {
                data[i] ^= KEY[keyIndex];
                keyIndex = (keyIndex + 1) % KEY.Length;
            }
            return data;
        }
    }
    
    internal class CustomMsgApp : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();
        protected override void OnInit()
        {
            m_UDPServer = new UDPSession();
            m_UDPServer.Init("127.0.0.1", 30000);
            m_UDPServer.Start();
        }

        protected override bool OnRun()
        {
            if (m_UDPServer == null || m_UDPServer.IsClosed())
            {
                return false;
            }
            m_RecvData.Clear();
            if (!m_UDPServer.GetRecvedData(m_RecvData))
            {
                return true;
            }
            while (m_RecvData.Count != 0)
            {
                var packet = m_RecvData.Dequeue();
                var data = MsgProto.XOR(packet.Data);
                var msg = new MsgProto.MoveToMsg();
                msg.Unserialize(data);
                Logger.LogInfo(
                    $"Msg From User({packet.ClientKey}): [PlayerID={msg.PlayerID},TargetPosition={msg.TargetPosition.ToString()},Speed={msg.Speed}]");
            }
            return true;
        }

        protected override void OnCleanup()
        {
            if (m_UDPServer != null)
            {
                m_UDPServer.Close();
                m_UDPServer = null;
            }
        }
    }
    
    internal static class CustomMsg
    {
        public static void Main(string[] args)
        {
            var app = new CustomMsgApp();
            app.Run();
        }
    }
}