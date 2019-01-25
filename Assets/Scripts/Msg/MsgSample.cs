using Network.Core;
using Network.UDP;
using System.Collections.Generic;
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
            return null;
        }
        public override void Unserialize(byte[] data)
        {

        }
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
                var data = m_RecvedData.Dequeue();
                ColoredLogger.Log("Msg From User: [" + Encoding.ASCII.GetString(data) + "]", ColoredLogger.LogColor.Yellow);
            }
        }
    }
    void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Say Hello"))
        {
            m_ClientSession.Send(Encoding.ASCII.GetBytes("Hello Server!"));
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
