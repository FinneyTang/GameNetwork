using Network.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Network.UDP
{
    public abstract class UDPSession : NetworkSession
    {
        protected UdpClient m_Socket;
        protected override void OnClose()
        {
            base.OnClose();
            if (m_Socket != null)
            {
                m_Socket.Close();
                m_Socket = null;
            }
        }
    }
    public class UDPListener : UDPSession
    {
        private Thread m_AcceptThread;
        public UDPListener()
        {
            m_SessionType = ESessionType.Server;
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
        protected override void OnStart()
        {
            base.OnStart();
            m_AcceptThread = CreateThread(AcceptThreadFunc);
        }
        protected override void OnClose()
        {
            if (m_AcceptThread != null)
            {
                m_AcceptThread.Join(2000);
                m_AcceptThread.Abort();
                m_AcceptThread = null;
            }
            base.OnClose();
        }
        private void AcceptThreadFunc()
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
                    if (m_Socket != null && m_Socket.Available > 0)
                    {
                        byte[] data = m_Socket.Receive(ref remoteIPEndPoint);
                        if (data != null && data.Length > 0)
                        {
                            ColoredLogger.Log(
                                "Msg From User: [" + Encoding.ASCII.GetString(data) + "]", ColoredLogger.LogColor.Yellow);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("error in accept thread:" + e.ToString());
                    return;
                }
            }
        }
    }
    public class UDPUser : UDPSession
    {
        private Thread m_SendThread;
        protected AutoResetEvent m_SendDataSignal;
        private Queue<string> m_PendingSendData;
        public UDPUser()
        {
            m_SessionType = ESessionType.User;
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
        public void Send(string msg)
        {
            lock(m_PendingSendData)
            {
                m_PendingSendData.Enqueue(msg);
                m_SendDataSignal.Set();
            }
        }
        protected override void OnStart()
        {
            base.OnStart();
            m_SendDataSignal = new AutoResetEvent(false);
            m_PendingSendData = new Queue<string>();
            m_SendThread = CreateThread(SendThreadFunc);
        }
        protected override void OnClose()
        {
            m_SendDataSignal.Set();
            if (m_SendThread != null)
            {
                m_SendThread.Join(1000);
                m_SendThread.Abort();
                m_SendThread = null;
            }
            base.OnClose();
        }
        private void SendThreadFunc()
        {
            Queue<string> dataToSend = new Queue<string>();
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
                        string msg = dataToSend.Dequeue();
                        byte[] data = Encoding.ASCII.GetBytes(msg);
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
}