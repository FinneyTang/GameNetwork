using Network.Core;
using Network.UDP;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MsgSampleServer : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private Queue<byte[]> m_RecvedData = new Queue<byte[]>();
    void Start()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
    }
    void Update()
    {
        if (m_ServerSession.GetRecvedData(m_RecvedData))
        {
            while (m_RecvedData.Count != 0)
            {
                var data = MsgProto.XOR(m_RecvedData.Dequeue());
                MsgProto.MoveToMsg msg = new MsgProto.MoveToMsg();
                msg.Unserialize(data);
                ColoredLogger.Log(
                    "Msg From User: [" +
                    string.Format("PlayerID={0},TargetPosition={1},Speed={2}", msg.PlayerID, msg.TargetPosition.ToString(), msg.Speed) +
                    "]", ColoredLogger.LogColor.Yellow);
            }
        }
    }
    void OnApplicationQuit()
    {
        if (m_ServerSession != null)
        {
            m_ServerSession.Close();
            m_ServerSession = null;
        }
    }
}
