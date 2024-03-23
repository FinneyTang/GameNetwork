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
    public class TCPClient : NetworkSession
    {
        public delegate byte[] EchoHandler();

        private TcpClient m_Client;
        private Thread m_SendThread;
        private readonly EchoHandler m_EchoHandler;

        public TCPClient(EchoHandler handler)
        {
            m_EchoHandler = handler;
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
            var ns = m_Client.GetStream();
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