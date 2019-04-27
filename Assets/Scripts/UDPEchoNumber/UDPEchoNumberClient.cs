using Network.Core;
using Network.UDP;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UDPEchoNumberClient : MonoBehaviour
{
    private UDPClient m_ClientSession = new UDPClient();
    private Queue<byte[]> m_RecvedData = new Queue<byte[]>();
    private uint m_LastNumber = 0;
    void Start()
    {
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }
    void Update()
    {
        if (m_ClientSession.GetRecvedData(m_RecvedData))
        {
            while (m_RecvedData.Count != 0)
            {
                var data = m_RecvedData.Dequeue();
                uint recvNumber = BitConverter.ToUInt32(data, 0);
                if(m_LastNumber == recvNumber)
                {
                    m_LastNumber++;
                }
            }
        }
    }
    void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Say Hello"))
        {
            m_ClientSession.Send(BitConverter.GetBytes(m_LastNumber));
        }
    }
    void OnApplicationQuit()
    {
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
