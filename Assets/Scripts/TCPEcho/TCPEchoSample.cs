using Network.Core;
using Network.TCP;
using System.IO;
using System.Text;
using UnityEngine;

public class TCPEchoSample : MonoBehaviour
{
    private TCPServer m_ServerSession = new TCPServer();
    private string m_PendingMsg = string.Empty;

    private TCPUser m_ClientSession = new TCPUser();
    void Start ()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.SetDataHandler((data, dataLen) =>
            {
                string msg = Encoding.ASCII.GetString(data, 0, dataLen);
                m_PendingMsg += msg;
                while (true)
                {
                    int endOfMsgPos = m_PendingMsg.IndexOf("!");
                    if(endOfMsgPos >= 0)
                    {
                        string helloMsg = m_PendingMsg.Substring(0, endOfMsgPos + 1);
                        ColoredLogger.Log("Msg From User: [" + helloMsg + "]", ColoredLogger.LogColor.Yellow);
                        m_PendingMsg = m_PendingMsg.Substring(endOfMsgPos+1);
                    }
                    else
                    {
                        break;
                    }
                }
                //ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
            });
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.SetEchoHandler(delegate ()
            {
                string msg = "Hello, Server!";
                ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
                return Encoding.ASCII.GetBytes(msg);
            });
            m_ClientSession.Start();
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
