using Network.UDP;
using Network.Core;
using System.Text;
using UnityEngine;

public class UDPEchoSample : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private UDPUser m_ClientSession = new UDPUser();
    void Start ()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.SetDataHandler((data, addr) =>
            {
                ColoredLogger.Log("Hello, User from " + addr.ToString(), ColoredLogger.LogColor.Green);
            });
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.SetEchoHandler(delegate ()
            {
                string msg = "Hello, Server";
                ColoredLogger.Log(msg, ColoredLogger.LogColor.Yellow);
                return Encoding.ASCII.GetBytes(msg);
            });
            m_ClientSession.Start();
        }
    }
    private void OnApplicationQuit()
    {
        if(m_ServerSession != null)
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
