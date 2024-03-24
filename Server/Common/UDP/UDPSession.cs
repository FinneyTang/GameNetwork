using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Common
{
    public class UDPSession : NetworkSession
    {
        private UdpClient m_Socket;
        private Thread m_ListenerThread;

        public struct UDPPacket
        {
            public string ClientKey;
            public byte[] Data;
        }
        
        private Queue<UDPPacket> m_RecvData;
        
        private class UDPClientInfo
        {
            public IPEndPoint Client;
            public Thread SendThread;
            public AutoResetEvent SendDataSignal;
            public Queue<byte[]> PendingSendData;
            
            public void Send(byte[] msg)
            {
                lock (PendingSendData)
                {
                    var copiedData = (byte[])msg.Clone();
                    PendingSendData.Enqueue(copiedData);
                    SendDataSignal.Set();
                }
            }
        }
        private Dictionary<string, UDPClientInfo> m_Client;

        protected override bool OnInit()
        {
            try
            {
                m_Socket = new UdpClient(m_Addr);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("failed to init socket: " + e);
            }

            return false;
        }

        protected override void OnStart()
        {
            base.OnStart();
            m_Client = new Dictionary<string, UDPClientInfo>();
            m_RecvData = new Queue<UDPPacket>();
            m_ListenerThread = CreateThread(ListenThreadFunc);
        }

        public void BroadcastToClients(byte[] data)
        {
            lock (m_Client)
            {
                foreach (var pair in m_Client)
                {
                    var clientInfo = pair.Value;
                    clientInfo.Send(data);
                }
            }
        }

        public void SendToClient(string clientKey, byte[] data)
        {
            lock (m_Client)
            {
                if (!m_Client.TryGetValue(clientKey, out var clientInfo))
                {
                    return;
                }
                clientInfo.Send(data);
            }
        }
        
        public bool GetRecvedData(Queue<UDPPacket> output)
        {
            lock(m_RecvData)
            {
                while (m_RecvData.Count != 0)
                {
                    var data = m_RecvData.Dequeue();
                    output.Enqueue(data);
                }
            }
            return output.Count > 0;
        }
        
        private void AddRecvData(UDPPacket packet)
        {
            lock(m_RecvData)
            {
                m_RecvData.Enqueue(packet);
            }
        }
        
        protected override void OnClose()
        {
            base.OnClose();
            if (m_ListenerThread != null)
            {
                m_ListenerThread.Join(500);
                m_ListenerThread.Abort();
                m_ListenerThread = null;
            }
            lock (m_Client)
            {
                foreach (var pair in m_Client)
                {
                    var clientInfo = pair.Value;
                    clientInfo.SendDataSignal.Set();
                    if (clientInfo.SendThread != null)
                    {
                        clientInfo.SendThread.Join(500);
                        clientInfo.SendThread.Abort();
                        clientInfo.SendThread = null;
                    }
                }
                m_Client.Clear();
            }
            if (m_Socket != null)
            {
                m_Socket.Close();
                m_Socket = null;
            }
        }
        
        private void ListenThreadFunc()
        {
            var remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                if (IsClosed())
                {
                    return;
                }
                try
                {
                    while(m_Socket != null && m_Socket.Available > 0)
                    {
                        if (IsClosed())
                        {
                            return;
                        }
                        var data = m_Socket.Receive(ref remoteIPEndPoint);
                        if (data.Length > 0)
                        {
                            var clientKey = remoteIPEndPoint.ToString();
                            lock (m_Client)
                            {
                                if (!m_Client.TryGetValue(clientKey, out var clientInfo))
                                {
                                    clientInfo = new UDPClientInfo
                                    {
                                        Client = new IPEndPoint(remoteIPEndPoint.Address, remoteIPEndPoint.Port),
                                        SendDataSignal = new AutoResetEvent(false),
                                        PendingSendData = new Queue<byte[]>()
                                    };
                                    clientInfo.SendThread = CreateThread(() => ClientSendThreadFunc(clientInfo));
                                    m_Client.Add(clientKey, clientInfo);
                                    Logger.LogInfo($"Add client {clientKey}");
                                }
                            }
                            AddRecvData(new UDPPacket()
                            {
                                ClientKey = clientKey, Data = data
                            });  
                        }
                    }
                    do
                    {
                        if (IsClosed())
                        {
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    while (m_Socket == null || m_Socket.Available <= 0);
                }
                catch (Exception e)
                {
                    Logger.LogError("error in listen thread:" + e);
                    return;
                }
            }
        }
        
        private void ClientSendThreadFunc(UDPClientInfo clientInfo)
        {
            var dataToSend = new Queue<byte[]>();
            while (true)
            {
                if (IsClosed())
                {
                    return;
                }
                clientInfo.SendDataSignal.WaitOne();
                try
                {
                    lock (clientInfo.PendingSendData)
                    {
                        while (clientInfo.PendingSendData.Count != 0)
                        {
                            var packet = clientInfo.PendingSendData.Dequeue();
                            dataToSend.Enqueue(packet);
                        }
                    }
                    while (dataToSend.Count != 0)
                    {
                        var data = dataToSend.Dequeue();
                        if (data != null && data.Length > 0)
                        {
                            m_Socket.Send(data, data.Length, clientInfo.Client);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("error in send thread:" + e);
                    return;
                }
            }
        }
    }
}