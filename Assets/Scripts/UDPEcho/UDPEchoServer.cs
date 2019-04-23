using Network.Core;
using Network.UDP;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UDPEchoServer : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private Queue<byte[]> m_RecvedData = new Queue<byte[]>();
    void Start ()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
    }
    void Update()
    {
        if(m_ServerSession.GetRecvedData(m_RecvedData))
        {
            while (m_RecvedData.Count != 0)
            {
                var data = m_RecvedData.Dequeue();
                ColoredLogger.Log("Msg From User: [" + Encoding.ASCII.GetString(data) + "]", ColoredLogger.LogColor.Yellow);
            }
        }
    }
    void OnApplicationQuit()
    {
        if(m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
    }
}
