using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace UDPEchoNumber
{
    internal class UDPEchoNumberApp : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();
        protected override void OnInit()
        {
            m_UDPServer = new UDPSession();
            m_UDPServer.Init("127.0.0.1", 30000);
            m_UDPServer.Start();
        }

        protected override bool OnRun(float curTimestamp)
        {
            if (m_UDPServer == null || m_UDPServer.IsClosed())
            {
                return false;
            }
            m_RecvData.Clear();
            if (!m_UDPServer.GetRecvedData(m_RecvData))
            {
                return true;
            }
            while (m_RecvData.Count != 0)
            {
                var packet = m_RecvData.Dequeue();
                var recvNumber = BitConverter.ToUInt32(packet.Data, 0);
                m_UDPServer.SendToClient(packet.ClientKey, BitConverter.GetBytes(recvNumber));
                Logger.LogInfo($"Msg From User({packet.ClientKey}): [{recvNumber}]");
            }
            return true;
        }

        protected override void OnCleanup()
        {
            if (m_UDPServer != null)
            {
                m_UDPServer.Close();
                m_UDPServer = null;
            }
        }
    }
    
    internal static class UDPEchoNumber
    {
        public static void Main(string[] args)
        {
            var app = new UDPEchoNumberApp();
            app.Run();
        }
    }
}