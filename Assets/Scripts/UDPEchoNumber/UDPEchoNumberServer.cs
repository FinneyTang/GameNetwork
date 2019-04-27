using Network.Core;
using Network.UDP;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UDPEchoNumberServer : MonoBehaviour
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
                uint recvNumber = BitConverter.ToUInt32(data, 0);
                m_ServerSession.Send(BitConverter.GetBytes(recvNumber));
                ColoredLogger.Log("Msg From User: [" + recvNumber + "]", ColoredLogger.LogColor.Yellow);
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
