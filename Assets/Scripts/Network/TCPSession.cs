using Network.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Network.TCP
{
    public class TCPSession : NetworkSession
    {
    }
    public class TCPServer : TCPSession
    {
        private TcpListener m_Listener;
        private Thread m_AcceptThread;
        private Action<byte[], int> m_DataHandler;
        public TCPServer()
        {
            m_SessionType = ESessionType.Server;
        }
        protected override bool OnInit(string addr, int port)
        {
            try
            {
                m_Listener = new TcpListener(m_Addr);
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
            m_Listener.Start();
            m_AcceptThread = CreateThread(AcceptThreadFunc);
        }
        public void SetDataHandler(Action<byte[], int> action)
        {
            m_DataHandler = action;
        }
        protected override void OnClose()
        {
            if (m_AcceptThread != null)
            {
                m_AcceptThread.Join(2000);
                m_AcceptThread.Abort();
                m_AcceptThread = null;
            }
            if (m_Listener != null)
            {
                m_Listener.Stop();
                m_Listener = null;
            }
            base.OnClose();
        }
        private void AcceptThreadFunc()
        {
            while (true)
            {
                if (IsClosed())
                {
                    return;
                }
                try
                {
                    if(m_Listener.Pending())
                    {
                        TcpClient client = m_Listener.AcceptTcpClient();
                        int bytesRead = 0;
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] chunks = new byte[10];
                            while (true)
                            {
                                if (IsClosed() || client.Connected == false)
                                {
                                    return;
                                }
                                if (stream.DataAvailable)
                                {
                                    bytesRead = stream.Read(chunks, 0, chunks.Length);
                                    if(bytesRead > 0 && m_DataHandler != null)
                                    {
                                        m_DataHandler.Invoke(chunks, bytesRead);
                                    }
                                }
                                else
                                {
                                    Thread.Sleep(50);
                                }
                            }
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
    public class TCPUser : TCPSession
    {
        public delegate byte[] EchoHandler();

        private TcpClient m_Client;
        private Thread m_SendThread;
        private EchoHandler m_EchoHandler;

        public TCPUser()
        {
            m_SessionType = ESessionType.User;
        }
        protected override bool OnInit(string addr, int port)
        {
            try
            {
                m_Client = new TcpClient();
                m_Client.Connect(m_Addr);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("AsClient error: " + e.ToString());
            }
            return false;
        }
        protected override void OnStart()
        {
            base.OnStart();
            m_SendThread = CreateThread(SendThreadFunc);
        }
        public void SetEchoHandler(EchoHandler action)
        {
            m_EchoHandler = action;
        }
        protected override void OnClose()
        {
            if (m_SendThread != null)
            {
                m_SendThread.Join(2000);
                m_SendThread.Abort();
                m_SendThread = null;
            }
            if (m_Client != null)
            {
                m_Client.Close();
                m_Client = null;
            }
            base.OnClose();
        }
        private void SendThreadFunc()
        {
            NetworkStream ns = m_Client.GetStream();
            while (true)
            {
                if (IsClosed() || m_Client.Connected == false)
                {
                    return;
                }
                try
                {
                    byte[] data = null;
                    if (m_EchoHandler != null)
                    {
                        data = m_EchoHandler.Invoke();
                    }
                    if (data != null && data.Length > 0)
                    {
                        ns.Write(data, 0, data.Length);
                    }
                    Thread.Sleep(10);
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