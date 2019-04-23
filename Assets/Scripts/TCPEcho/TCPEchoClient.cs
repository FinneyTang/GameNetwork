using Network.Core;
using Network.TCP;
using System.IO;
using System.Text;
using UnityEngine;

public class TCPEchoClient : MonoBehaviour
{
    private TCPClient m_ClientSession;
    void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Connect"))
        {
            if(m_ClientSession == null)
            {
                m_ClientSession = new TCPClient();
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
