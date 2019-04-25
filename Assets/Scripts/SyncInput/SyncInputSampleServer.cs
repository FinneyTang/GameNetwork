using Network.UDP;
using System.Collections.Generic;
using UnityEngine;

public class SyncInputSampleServer : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private Queue<byte[]> m_ServerRecvedData = new Queue<byte[]>();
    void Start()
    {
        Time.fixedDeltaTime = 0.1f;

        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
    }
    void Update()
    {
        ServerUpdate();
    }
    void FixedUpdate()
    {
        ServerFixedUpdate();
    }
    private bool m_HasClientConnected = false;
    private int m_CurrentFrame = 0;
    private InputMsg m_ClientInput = new InputMsg();
    void ServerUpdate()
    {
        if (m_HasClientConnected)
        {
            if (m_ServerSession.GetRecvedData(m_ServerRecvedData))
            {
                while (m_ServerRecvedData.Count != 0)
                {
                    m_ClientInput.Unserialize(m_ServerRecvedData.Dequeue());
                }
            }
        }
    }
    void ServerFixedUpdate()
    {
        if (m_HasClientConnected == false)
        {
            if (m_ServerSession.GetRecvedData(m_ServerRecvedData))
            {
                while (m_ServerRecvedData.Count != 0)
                {
                    m_HasClientConnected = true;
                    m_ServerRecvedData.Dequeue();
                }
            }
        }
        else
        {
            m_CurrentFrame++;
            m_ServerSession.Send(m_ClientInput.Serialize());
        }
    }
    private void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        GUI.Label(new Rect(margin, margin, Screen.width - 2 * margin, 20), m_CurrentFrame.ToString());
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
