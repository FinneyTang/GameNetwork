using System;
using System.Collections.Generic;
using Common;

namespace Ping
{
    internal class PingApp : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();
        protected override void OnInit()
        {
            m_UDPServer = new UDPSession();
            m_UDPServer.Init("127.0.0.1", 30000);
            m_UDPServer.Start();

            RandomUtils.SetSeed((int)(TimeUtils.GetTimeStamp() * 1000));
        }

        protected override bool OnRun()
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
                DelayCall(() =>
                {
                    m_UDPServer.SendToClient(packet.ClientKey, packet.Data);
                }, RandomUtils.Range(0.1f, 0.3f));
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
    
    internal static class Ping
    {
        public static void Main(string[] args)
        {
            var app = new PingApp();
            app.Run();
        }
    }
}