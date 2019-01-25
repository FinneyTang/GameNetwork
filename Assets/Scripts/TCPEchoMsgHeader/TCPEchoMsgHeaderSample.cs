using Network.Core;
using Network.TCP;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public class TCPEchoMsgHeaderSample : MonoBehaviour
{
    private TCPServer m_ServerSession = new TCPServer();
    private MemoryStream m_PendingStream = new MemoryStream();

    private TCPUser m_ClientSession = new TCPUser();
    void Start ()
    {
        if (m_ServerSession.Init("127.0.0.1", 30000))
        {
            m_ServerSession.SetDataHandler((data, dataLen) =>
            {
                MemoryStream recvStream = new MemoryStream();
                if (m_PendingStream.Length > 0)
                {
                    recvStream.Write(m_PendingStream.GetBuffer(), 0, (int)m_PendingStream.Length);
                    m_PendingStream.SetLength(0);
                }
                recvStream.Write(data, 0, dataLen);
                int avaliableCount = (int)recvStream.Length;
                recvStream.Seek(0, SeekOrigin.Begin);
                BinaryReader reader = new BinaryReader(recvStream);
                int headerSize = 4;
                while (true)
                {
                    if(avaliableCount < headerSize)
                    {
                        m_PendingStream.SetLength(avaliableCount);
                        reader.Read(m_PendingStream.GetBuffer(), 0, avaliableCount);
                        break;
                    }
                    int len = reader.ReadInt32();
                    avaliableCount -= headerSize;
                    if (avaliableCount < len)
                    {
                        recvStream.Seek(-headerSize, SeekOrigin.Current);
                        int remainingCount = avaliableCount + headerSize;
                        m_PendingStream.SetLength(remainingCount);
                        reader.Read(m_PendingStream.GetBuffer(), 0, remainingCount);
                        break;
                    }
                    else
                    {
                        byte[] msgBytes = reader.ReadBytes(len);
                        avaliableCount -= len;
                        ColoredLogger.Log("Msg From User: [" + Encoding.ASCII.GetString(msgBytes, 0, len) + "]", ColoredLogger.LogColor.Yellow);
                    }
                }
            });
            m_ServerSession.Start();
        }
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.SetEchoHandler(delegate ()
            {
                string msg = "Hello, Server!";
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
