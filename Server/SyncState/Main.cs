using System;
using System.Collections.Generic;
using Common;

namespace SyncState
{
    internal class SyncStateApp : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();

        private class ClientObject
        {
            public Vector3 Vel;
            public Vector3 Pos;
            public Vector3 Fwd;
        }
        private readonly Dictionary<string, ClientObject> m_Objects = new Dictionary<string, ClientObject>();
        private readonly InputMsg m_ClientInput = new InputMsg();

        private float m_LastTimestamp = -1f;
        private float m_NextSyncTime;
        
        private void ServerUpdate(float curTimestamp, float deltaTime)
        {
            //update object pos
            foreach (var pair in m_Objects)
            {
                var clientObj = pair.Value;
                clientObj.Pos += (clientObj.Vel * deltaTime);
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
        }
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
            //handle client input
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
                        }
                    };
                    m_Objects[packet.ClientKey] = clientObj;
                }
                m_ClientInput.Unserialize(packet.Data);
                var targetFacing = m_ClientInput.Dir.Normalize();
                clientObj.Vel = targetFacing * 6f;
                if (!targetFacing.IsZero())
                {
                    clientObj.Fwd = targetFacing;
                }
            }
            //update server
            var deltaTime = 0f;
            if (m_LastTimestamp >= 0)
            {
                deltaTime = curTimestamp - m_LastTimestamp;   
            }
            ServerUpdate(curTimestamp, deltaTime);
            m_LastTimestamp = curTimestamp;
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
    
    internal static class SyncState
    {
        public static void Main(string[] args)
        {
            var app = new SyncStateApp();
            app.Run();
        }
    }
}