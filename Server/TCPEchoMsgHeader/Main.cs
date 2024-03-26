using System;
using System.IO;
using System.Text;
using Common;

namespace TCPEchoMsgHeader
{
    internal class TCPEchoMsgHeaderApp : AppBase
    {
        private TCPSession m_TCPServer;
        private readonly MemoryStream m_PendingStream = new MemoryStream();
        private void TCPDataHandler(byte[] data, int dataLen)
        {
            var recvStream = new MemoryStream();
            if (m_PendingStream.Length > 0)
            {
                recvStream.Write(m_PendingStream.GetBuffer(), 0, (int)m_PendingStream.Length);
                m_PendingStream.SetLength(0);
            }
            recvStream.Write(data, 0, dataLen);
            var avaliableCount = (int)recvStream.Length;
            recvStream.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(recvStream);
            const int headerSize = 4;
            while (true)
            {
                if(avaliableCount < headerSize)
                {
                    m_PendingStream.SetLength(avaliableCount);
                    reader.Read(m_PendingStream.GetBuffer(), 0, avaliableCount);
                    break;
                }
                var len = reader.ReadInt32();
                avaliableCount -= headerSize;
                if (avaliableCount < len)
                {
                    recvStream.Seek(-headerSize, SeekOrigin.Current);
                    var remainingCount = avaliableCount + headerSize;
                    m_PendingStream.SetLength(remainingCount);
                    reader.Read(m_PendingStream.GetBuffer(), 0, remainingCount);
                    break;
                }
                else
                {
                    var msgBytes = reader.ReadBytes(len);
                    avaliableCount -= len;
                    Logger.LogInfo("Msg From User: [" + Encoding.ASCII.GetString(msgBytes, 0, len) + "]");
                }
            }
        }

        protected override void OnInit()
        {
            m_TCPServer = new TCPSession(TCPDataHandler);
            m_TCPServer.Init("127.0.0.1", 30000);
            m_TCPServer.Start();
        }

        protected override void OnCleanup()
        {
            if(m_TCPServer != null)
            {
                m_TCPServer.Close();
                m_TCPServer = null;
            }
        }
    }

    internal static class TCPEchoMsgHeader
    {
        public static void Main(string[] args)
        {
            var app = new TCPEchoMsgHeaderApp();
            app.Run();
        }
    }
}