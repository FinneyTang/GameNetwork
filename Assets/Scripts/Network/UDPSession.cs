using Network.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Network.UDP
{
    public class UDPClient : NetworkSession
    {
        private UdpClient m_Socket;
        private Thread m_RecieveThread;
        private Thread m_SendThread;

        private AutoResetEvent m_SendDataSignal;
        private Queue<byte[]> m_PendingSendData;

        private Queue<byte[]> m_RecvedData;
        private string m_ClientKey;

        protected override bool OnInit()
        {
            try
            {
                m_Socket = new UdpClient();
                m_Socket.Connect(m_Addr);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("AsClient error: " + e);
            }
            return false;
        }
        protected override void OnStart()
        {
            base.OnStart();

            m_ClientKey = m_Socket.Client.LocalEndPoint.ToString();
            
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

        public string ClientKey => m_ClientKey;
        
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
            IPEndPoint remoteIPEndPoint = null;
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
            var dataToSend = new Queue<byte[]>();
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
                        var data = dataToSend.Dequeue();
                        if (data != null && data.Length > 0)
                        {
                            m_Socket.Send(data, data.Length);
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
}