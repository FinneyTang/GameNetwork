using Network.UDP;
using UnityEngine;

public class UDPEchoSample : MonoBehaviour
{
    private UDPListener m_ServerSession = new UDPListener();
    private UDPUser m_ClientSession = new UDPUser();
    void Start ()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }
    void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Say Hello"))
        {
            m_ClientSession.Send("Hello Server!");
        }
    }
    void OnApplicationQuit()
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
