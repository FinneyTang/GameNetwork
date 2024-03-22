using Network.Core;
using Network.TCP;
using System.IO;
using System.Text;
using UnityEngine;

public class TCPEchoMsgHeaderClient : MonoBehaviour
{
    private readonly string[] CLIENT_NAMES = new string[]
    {
        "Tom", "Mary", "Jerry", "Finney", "William", "Elizabeth"
    };
    private TCPClient m_ClientSession;
    private int m_ClientNameIdx;

    private void Update()
    {
        m_ClientNameIdx = Random.Range(0, CLIENT_NAMES.Length);
    }

    private void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Connect"))
        {
            if (m_ClientSession == null)
            {
                m_ClientSession = new TCPClient();
                if (m_ClientSession.Init("127.0.0.1", 30000))
                {
                    m_ClientSession.SetEchoHandler(delegate ()
                    {
                        string msg = string.Format("Hello, Server! I'm {0}", CLIENT_NAMES[m_ClientNameIdx]);
                        MemoryStream stream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(stream);
                        byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
                        writer.Write(msgBytes.Length);
                        writer.Write(msgBytes);
                        ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
                        return stream.ToArray();
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
