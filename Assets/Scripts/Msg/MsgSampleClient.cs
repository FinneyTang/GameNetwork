using Network.Core;
using Network.UDP;
using System;
using UnityEngine;

public class MsgSampleClient : MonoBehaviour
{
    private UDPClient m_ClientSession = new UDPClient();
    void Start()
    {
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }
    void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Send MoveToMsg"))
        {
            MsgProto.MoveToMsg msg = new MsgProto.MoveToMsg();
            msg.PlayerID = 1;
            msg.TargetPosition = new Vector3(-1f, 2f, 3.5f);
            msg.Speed = 2f;
            byte[] data = MsgProto.XOR(msg.Serialize());
            ColoredLogger.Log(BitConverter.ToString(data).Replace("-", " "), ColoredLogger.LogColor.Green);
            m_ClientSession.Send(data);
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
