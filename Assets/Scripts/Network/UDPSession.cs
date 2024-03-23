using Network.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Network.UDP
{
    public abstract class UDPSession : NetworkSession
    {
        protected UdpClient m_Socket;
        protected Thread m_RecieveThread;
        protected Thread m_SendThread;

        protected AutoResetEvent m_SendDataSignal;
        protected Queue<byte[]> m_PendingSendData;

        protected Queue<byte[]> m_RecvedData;
        protected bool m_IsServer;

        public UDPSession(bool isServer)
        {
            m_IsServer = isServer;
        }
        protected override void OnStart()
        {
            base.OnStart();
            m_SendDataSignal = new AutoResetEvent(false);
            m_PendingSendData = new Queue<byte[]>();
            m_RecvedData = new Queue<byte[]>();
            m_RecieveThread = CreateThread(RecieveThreadFunc);
            m_SendThread = CreateThread(SendThreadFunc);
        }
        protected override void OnClose()
        {
            base.OnClose();
            if (m_RecieveThread != null)
            {
                m_RecieveThread.Join(500);
                m_RecieveThread.Abort();
                m_RecieveThread = null;
            }
            m_SendDataSignal.Set();
            if (m_SendThread != null)
            {
                m_SendThread.Join(500);
                m_SendThread.Abort();
                m_SendThread = null;
            }
            if (m_Socket != null)
            {
                m_Socket.Close();
                m_Socket = null;
            }
        }
        public void Send(byte[] msg)
        {
            lock (m_PendingSendData)
            {
                m_PendingSendData.Enqueue(msg);
                m_SendDataSignal.Set();
            }
        }
        public bool GetRecvedData(Queue<byte[]> output)
        {
            lock(m_RecvedData)
            {
                while (m_RecvedData.Count != 0)
                {
                    var data = m_RecvedData.Dequeue();
                    output.Enqueue(data);
                }
            }
            return output.Count > 0;
        }
        private void RecieveThreadFunc()
        {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
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
                        byte[] data = m_Socket.Receive(ref remoteIPEndPoint);
                        if (data != null && data.Length > 0)
                        {
                            if (m_IsServer)
                            {
                                m_Addr = remoteIPEndPoint;
                            }
                            lock (m_RecvedData)
                            {
                                m_RecvedData.Enqueue(data);
                            }
                        }
                    }
                    do
                    {
                        if (IsClosed())
                        {
                            return;
                        }
                        Thread.Sleep(10);
                    }
                    while (m_Socket == null || m_Socket.Available <= 0);
                }
                catch (Exception e)
                {
                    Debug.LogError("error in accept thread:" + e.ToString());
                    return;
                }
            }
        }
        private void SendThreadFunc()
        {
            Queue<byte[]> dataToSend = new Queue<byte[]>();
            while (true)
            {
                if (IsClosed())
                {
                    return;
                }
                m_SendDataSignal.WaitOne();
                try
                {
                    lock (m_PendingSendData)
                    {
                        while (m_PendingSendData.Count != 0)
                        {
                            var packet = m_PendingSendData.Dequeue();
                            dataToSend.Enqueue(packet);
                        }
                    }
                    while (dataToSend.Count != 0)
                    {
                        byte[] data = dataToSend.Dequeue();
                        if (data != null && data.Length > 0)
                        {
                            m_Socket.Send(data, data.Length, m_Addr);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("error in send thread:" + e.ToString());
                    return;
                }
            }
        }
    }
    
    public class UDPListener : UDPSession
    {
        public UDPListener() : base(true)
        {
        }
        protected override bool OnInit(string addr, int port)
        {
            try
            {
                m_Socket = new UdpClient(port);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("AsServer error: " + e.ToString());
            }
            return false;
        }
    }
    
    public class UDPClient : UDPSession
    {
        public UDPClient() : base(false)
        {
        }
        protected override bool OnInit(string addr, int port)
        {
            try
            {
                m_Socket = new UdpClient();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("AsClient error: " + e.ToString());
            }
            return false;
        }
    }
}