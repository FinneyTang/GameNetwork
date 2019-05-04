using Network.UDP;
using System.Collections.Generic;
using UnityEngine;

public class SyncState2SampleServer : MonoBehaviour
{
    public Transform ServerObjectTF;

    private UDPListener m_ServerSession = new UDPListener();
    private Queue<byte[]> m_ServerRecvedData = new Queue<byte[]>();

    void Start()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
    }
    void Update()
    {
        ServerUpdate();
    }
    private bool m_HasClientConnected = false;
    private float m_LastTimeStamp;
    void ServerUpdate()
    {
        if (m_ServerSession.GetRecvedData(m_ServerRecvedData))
        {
            while (m_ServerRecvedData.Count != 0)
            {
                var data = m_ServerRecvedData.Dequeue();
                if (m_HasClientConnected == false)
                {
                    m_HasClientConnected = true;
                }
                else
                {
                    UploadStateMsg msg = new UploadStateMsg();
                    msg.Unserialize(data);
                    if (msg.TimeStamp > m_LastTimeStamp)
                    {
                        ServerObjectTF.position = msg.TargetPosition;
                        ServerObjectTF.rotation = msg.TargetOrientation;

                        m_LastTimeStamp = msg.TimeStamp;
                    }
                }
            }
        }
    }
    //----------------------------------------------------------
    void OnApplicationQuit()
    {
        if (m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
    }
}
