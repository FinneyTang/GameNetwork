using System.Collections.Generic;
using Common;

namespace SyncInput
{
    internal class SyncInputApp : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();
        
        private readonly InputMsg m_ClientInput = new InputMsg();
        private readonly FrameClientInputsMsg m_FrameClientInputs = new FrameClientInputsMsg();
        private readonly Dictionary<string, FrameClientInputsMsg.ClientInputData> m_CachedClientInputs = new Dictionary<string, FrameClientInputsMsg.ClientInputData>();
        
        private int m_FrameCount = 0;
        
        protected override void OnInit()
        {
            SetTargetFPS(15);
            
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
            m_UDPServer.GetRecvedData(m_RecvData);
            
            //handle client input
            while (m_RecvData.Count != 0)
            {
                var packet = m_RecvData.Dequeue();
                m_ClientInput.Unserialize(packet.Data);
                if(!m_CachedClientInputs.TryGetValue(packet.ClientKey, out var clientInput))
                {
                    clientInput = new FrameClientInputsMsg.ClientInputData()
                    {
                        ClientKey = packet.ClientKey
                    };
                    m_CachedClientInputs[packet.ClientKey] = clientInput;
                }
                clientInput.X = m_ClientInput.X;
                clientInput.Y = m_ClientInput.Y;
            }
            
            //construct frame message and push it to client
            m_FrameCount++;
            m_FrameClientInputs.FrameCount = m_FrameCount;
            foreach (var pair in m_CachedClientInputs)
            {
                m_FrameClientInputs.ClientInputs.Add(pair.Value);
            }
            m_UDPServer.BroadcastToClients(m_FrameClientInputs.Serialize());
            
            //clear last inputs
            m_CachedClientInputs.Clear();
            m_FrameClientInputs.ClientInputs.Clear();
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

    internal static class SyncInput
    {
        public static void Main(string[] args)
        {
            var app = new SyncInputApp();
            app.Run();
        }
    }
}