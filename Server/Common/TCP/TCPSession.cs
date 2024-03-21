using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Common
{
    public class TCPSession : NetworkSession
    {
        private TcpListener m_Listener;
        private Thread m_AcceptThread;

        private struct TCPClientInfo
        {
            public TcpClient Client;
            public Thread RecvThread;
        }
        private readonly List<TCPClientInfo> m_Clients = new List<TCPClientInfo>();
        private readonly Action<byte[], int> m_DataHandler;
        
        public TCPSession(Action<byte[], int> dataHandler)
        {
            m_DataHandler = dataHandler;
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
                Logger.LogError("error: " + e);
            }
            return false;
        }
        protected override void OnStart()
        {
            base.OnStart();
            m_Listener.Start();
            m_AcceptThread = CreateThread(AcceptThreadFunc);
            Logger.LogInfo("TCPServer started at " + m_Addr.Address + ":" + m_Addr.Port);
        }
        protected override void OnClose()
        {
            Logger.LogInfo("TCPServer closed");
            if (m_AcceptThread != null)
            {
                Logger.LogInfo("Join accept thread");
                m_AcceptThread.Join(2000);
                m_AcceptThread.Abort();
                m_AcceptThread = null;
            }
            lock (m_Clients)
            {
                Logger.LogInfo("Join client threads");
                foreach (var client in m_Clients)
                {
                    if (client.RecvThread != null)
                    {
                        client.RecvThread.Join(2000);
                        client.RecvThread.Abort();
                    }
                    if (client.Client != null)
                    {
                        client.Client.Close();
                    }
                }
                m_Clients.Clear();
            }
            if (m_Listener != null)
            {
                Logger.LogInfo("Stop listener");
                m_Listener.Stop();
                m_Listener = null;
            }
            base.OnClose();
        }

        private void RecvThreadFunc(TcpClient client)
        {
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
                        var bytesRead = stream.Read(chunks, 0, chunks.Length);
                        if(bytesRead > 0 && m_DataHandler != null)
                        {
                            m_DataHandler.Invoke(chunks, bytesRead);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
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
                        lock (m_Clients)
                        {
                            var thread = new Thread(() => RecvThreadFunc(client));
                            thread.IsBackground = true;
                            thread.Priority = ThreadPriority.Normal;
                            thread.Start(); 
                            m_Clients.Add(new TCPClientInfo { Client = client, RecvThread = thread });
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("error in accept thread:" + e);
                    return;
                }
            }
        }
    }
}