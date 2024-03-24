using Network.Ping;
using Network.UDP;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PingSampleClient : MonoBehaviour
{
    private readonly PingUtil m_Ping = new PingUtil();
    private UDPClient m_ClientSession = new UDPClient();
    private readonly Queue<byte[]> m_ClientRecvedData = new Queue<byte[]>();
    private float m_NextPingSentTime = 0;

    private void Start ()
    {
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }

    private void Update ()
    {
		if(Time.time > m_NextPingSentTime)
        {
            m_Ping.PingSent(Time.time);
            m_ClientSession.Send(BitConverter.GetBytes(Time.time));
            m_NextPingSentTime = Time.time + 1f;
        }
        if(m_ClientSession.GetRecvedData(m_ClientRecvedData))
        {
            while (m_ClientRecvedData.Count != 0)
            {
                var data = m_ClientRecvedData.Dequeue();
                m_Ping.PingBack(BitConverter.ToSingle(data, 0));
            }
        }
    }

    private GUIStyle m_GUIStyle;
    private void OnGUI()
    {
        if (m_GUIStyle == null)
        {
            m_GUIStyle = new GUIStyle
            {
                fontSize = 100,
                normal =
                {
                    textColor = Color.yellow
                },
                alignment = TextAnchor.MiddleCenter
            };
        }
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), m_Ping.CurPing.ToString("f0"), m_GUIStyle);
    }

    private void OnApplicationQuit()
    {
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
