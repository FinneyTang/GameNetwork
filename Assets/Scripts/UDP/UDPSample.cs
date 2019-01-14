using Network.UDP;
using System.Text;
using UnityEngine;

public class UDPSample : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private UDPUser m_ClientSession = new UDPUser();
    void Start ()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.SetDataHandler((data, addr) =>
            {
                Debug.Log("<color=green>Hello, User from " + addr.ToString() + "</color>");
            });
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.SetEchoHandler(delegate ()
            {
                string msg = "Hello, Server";
                Debug.Log("<color=yellow>" + msg + "</color>");
                return Encoding.ASCII.GetBytes(msg);
            });
            m_ClientSession.Start();
        }
    }
    private void OnApplicationQuit()
    {
        if(m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}
