using System.Net;
using System.Threading;

namespace Network.Core
{
    public abstract class NetworkSession
    {
        protected enum ESessionType
        {
            None, Server, User
        }
        protected ESessionType m_SessionType = ESessionType.None;
        protected bool m_IsClosed;
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
            m_IsClosed = true;
            OnClose();
        }
        public bool IsClient()
        {
            return m_SessionType == ESessionType.User;
        }
        public bool IsServer()
        {
            return m_SessionType == ESessionType.Server;
        }
        public bool IsClosed()
        {
            return m_IsClosed;
        }
        public Thread CreateThread(ThreadStart threadFunc)
        {
            Thread t = new Thread(threadFunc);
            t.IsBackground = true;
            t.Priority = ThreadPriority.Normal;
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
