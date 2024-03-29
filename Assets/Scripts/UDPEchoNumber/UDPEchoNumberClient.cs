﻿using Network.Core;
using Network.UDP;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UDPEchoNumberClient : MonoBehaviour
{
    private UDPClient m_ClientSession = new UDPClient();
    private readonly Queue<byte[]> m_RecvedData = new Queue<byte[]>();
    private uint m_LastNumber = 0;

    private void Start()
    {
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }

    private void Update()
    {
        if (m_ClientSession.GetRecvedData(m_RecvedData))
        {
            while (m_RecvedData.Count != 0)
            {
                var data = m_RecvedData.Dequeue();
                var recvNumber = BitConverter.ToUInt32(data, 0);
                if(m_LastNumber == recvNumber)
                {
                    m_LastNumber++;
                    ColoredLogger.Log($"recv {recvNumber}, increase to {m_LastNumber}", ColoredLogger.LogColor.Yellow);
                }
            }
        }
    }

    private void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Say Hello"))
        {
            ColoredLogger.Log($"send {m_LastNumber}", ColoredLogger.LogColor.Green);
            m_ClientSession.Send(BitConverter.GetBytes(m_LastNumber));
        }
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
