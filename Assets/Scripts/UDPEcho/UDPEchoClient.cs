using Network.UDP;
using System.Text;
using Network.Core;
using UnityEngine;

public class UDPEchoClient : MonoBehaviour
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
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Say Hello"))
        {
            const string msg = "Hello, Server!";
            m_ClientSession.Send(Encoding.ASCII.GetBytes(msg));
            ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
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
