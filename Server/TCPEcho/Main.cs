using System;
using System.Text;
using Common;

namespace TCPEcho
{
    internal class TCPEchoServerApp : AppBase
    {
        private TCPSession m_TCPServer;
        private string m_PendingMsg = string.Empty;
        private void TCPDataHandler(byte[] data, int dataLen)
        {
            string msg = Encoding.ASCII.GetString(data, 0, dataLen);
            m_PendingMsg += msg;
            while (true)
            {
                int endOfMsgPos = m_PendingMsg.IndexOf("!", StringComparison.Ordinal);
                if(endOfMsgPos >= 0)
                {
                    string helloMsg = m_PendingMsg.Substring(0, endOfMsgPos + 1);
                    Logger.LogInfo("Msg From User: [" + helloMsg + "]");
                    m_PendingMsg = m_PendingMsg.Substring(endOfMsgPos + 1);
                }
                else
                {
                    break;
                }
            }
        }

        protected override void OnRun()
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
    
    internal static class TCPEchoServer
    {
        public static void Main(string[] args)
        {
            var app = new TCPEchoServerApp();
            app.Run();
        }
    }
}