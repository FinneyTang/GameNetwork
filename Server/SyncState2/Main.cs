using System.Collections.Generic;
using Common;

namespace SyncState2
{
    internal class SyncState2App : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();

        private class ClientObject
        {
            public Vector3 Pos;
            public Vector3 Fwd;
            public float LastTimeStamp;
        }
        private readonly Dictionary<string, ClientObject> m_Objects = new Dictionary<string, ClientObject>();
        private readonly UploadStateMsg m_ClientUploadMsg = new UploadStateMsg();
        
        private float m_NextSyncTime;
        
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
            m_UDPServer.GetRecvedData(m_RecvData);
            
            //handle client upload state
            while (m_RecvData.Count != 0)
            {
                var packet = m_RecvData.Dequeue();
                if(!m_Objects.TryGetValue(packet.ClientKey, out var clientObj))
                {
                    clientObj = new ClientObject()
                    {
                        Pos = new Vector3(),
                        Fwd = new Vector3()
                        {
                            z = 1,
                        },
                        LastTimeStamp = -1f
                    };
                    m_Objects[packet.ClientKey] = clientObj;
                }
                m_ClientUploadMsg.Unserialize(packet.Data);
                if(m_ClientUploadMsg.TimeStamp > clientObj.LastTimeStamp)
                {
                    clientObj.LastTimeStamp = m_ClientUploadMsg.TimeStamp;
                    clientObj.Pos = m_ClientUploadMsg.TargetPosition;
                    clientObj.Fwd = m_ClientUploadMsg.TargetForward;
                }
            }
            
            //send all object states to all clients
            if (curTimestamp > m_NextSyncTime)
            {
                foreach (var pair in m_Objects)
                {
                    var clientObj = pair.Value;
                    var msg = new StateMsg
                    {
                        ClientKey = pair.Key,
                        TargetPosition = clientObj.Pos,
                        TargetForward = clientObj.Fwd,
                        TimeStamp = curTimestamp
                    };
                    m_UDPServer.BroadcastToClients(msg.Serialize());   
                }
                m_NextSyncTime = curTimestamp + 0.2f;
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

    internal static class SyncState2
    {
        public static void Main(string[] args)
        {
            var app = new SyncState2App();
            app.Run();
        }
    }
}