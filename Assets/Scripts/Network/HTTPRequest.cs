using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Network.HTTP
{
    class HTTPRequest
    {
        private UnityWebRequest m_Request;
        private UnityWebRequestAsyncOperation m_AsyncOp;
        private Action<bool, byte[]> m_RequestFinishedAction;
        private string m_LastError = string.Empty;
        public string LastError
        {
            get
            {
                return m_LastError;
            }
        }
        public void Get(string addr, string cmd, Action<bool, byte[]> onRequestFinished, params object[] args)
        {
            if (args.Length % 2 != 0)
            {
                onRequestFinished.Invoke(false, null);
                return;
            }
            int argLen = args.Length / 2;
            string paramStr = "";
            for (int i = 0; i < argLen; i++)
            {
                if (i == 0)
                {
                    paramStr += "?";
                }
                else
                {
                    paramStr += "&";
                }
                paramStr += (args[i * 2].ToString() + "=" + Uri.EscapeDataString(args[i * 2 + 1].ToString()));
            }
            string url = Uri.EscapeUriString(addr) + cmd + paramStr;
            m_Request = UnityWebRequest.Get(url);
            m_RequestFinishedAction = onRequestFinished;
            m_AsyncOp = m_Request.SendWebRequest();
        }
        public void CheckPendingRequest()
        {
            if(m_AsyncOp == null)
            {
                return;
            }
            if(m_AsyncOp.isDone)
            {
                bool hasError = m_Request.isNetworkError || m_Request.isHttpError;
                m_LastError = m_Request.error;
                m_RequestFinishedAction.Invoke(!hasError, m_Request.downloadHandler.data);
                Dispose();
            }
        }
        private void Dispose()
        {
            m_Request.Dispose();
            m_RequestFinishedAction = null;
            m_AsyncOp = null;
        }
    }
}
