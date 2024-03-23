using System.Net;
using System.Threading;

namespace Network.Core
{
    public abstract class NetworkSession
    {
        private bool m_IsClosed;
        protected IPEndPoint m_Addr;

        public bool Init(string addr, int port)
        {
            m_Addr = new IPEndPoint(IPAddress.Parse(addr), port);
            return OnInit(addr, port);
        }
        public void Start()
        {
            OnStart();
        }
        public void Close()
        {
            if (m_IsClosed == true)
            {
                return;
            }
            m_IsClosed = true;
            OnClose();
        }

        protected bool IsClosed()
        {
            return m_IsClosed;
        }

        protected Thread CreateThread(ThreadStart threadFunc)
        {
            var t = new Thread(threadFunc)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            t.Start();
            return t;
        }
        protected virtual bool OnInit(string addr, int port)
        {
            return false;
        }
        protected virtual void OnStart()
        {
        }
        protected virtual void OnClose()
        {
        }
    }
}
