using Network.Core;
using Network.UDP;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MsgSample : MonoBehaviour
{
    abstract class MsgBase
    {
        public abstract byte[] Serialize();
        public abstract void Unserialize(byte[] data);
    }
    class MoveToMsg : MsgBase
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
    private readonly byte[] KEY = new byte[]{ 0x36, 0x7F, 0x45 };
    private byte[] XOR(byte[] data, byte[] key)
    {
        int keyIndex = 0;
        for(int i = 0; i < data.Length; i++)
        {
            data[i] ^= key[keyIndex];
            keyIndex = (keyIndex + 1) % key.Length;
        }
        return data;
    }


    private UDPListener m_ServerSession = new UDPListener();
    private UDPUser m_ClientSession = new UDPUser();
    private Queue<byte[]> m_RecvedData = new Queue<byte[]>();
    void Start()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }
    void Update()
    {
        if (m_ServerSession.GetRecvedData(m_RecvedData))
        {
            while (m_RecvedData.Count != 0)
            {
                var data = XOR(m_RecvedData.Dequeue(), KEY);
                MoveToMsg msg = new MoveToMsg();
                msg.Unserialize(data);
                ColoredLogger.Log(
                    "Msg From User: [" + 
                    string.Format("PlayerID={0},TargetPosition={1},Speed={2}", msg.PlayerID, msg.TargetPosition.ToString(), msg.Speed) + 
                    "]", ColoredLogger.LogColor.Yellow);
            }
        }
    }
    void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Send MoveToMsg"))
        {
            MoveToMsg msg = new MoveToMsg();
            msg.PlayerID = 1;
            msg.TargetPosition = new Vector3(-1f, 2f, 3.5f);
            msg.Speed = 2f;
            byte[] data = XOR(msg.Serialize(), KEY);
            m_ClientSession.Send(data);
        }
    }
    void OnApplicationQuit()
    {
        if (m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
