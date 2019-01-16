using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network.HTTP
{
    class HTTPRequest
    {
        private WWW m_Request;
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
                m_LastError = "invalid arguments";
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
            m_RequestFinishedAction = onRequestFinished;
            m_Request = new WWW(url);
        }
        public void Post(string addr, string cmd, Action<bool, byte[]> onRequestFinished, byte[] data, Dictionary<string, string> header)
        {
            string url = Uri.EscapeUriString(addr) + cmd;
            m_RequestFinishedAction = onRequestFinished;
            if(data != null && data.Length == 0)
            {
                data = null;
            }
            m_Request = new WWW(url, data, header);
        }
        public void CheckPendingRequest()
        {
            if(m_Request == null)
            {
                return;
            }
            if(m_Request.isDone)
            {
                m_LastError = m_Request.error;
                m_RequestFinishedAction.Invoke(string.IsNullOrEmpty(m_LastError), m_Request.bytes);
                Dispose();
            }
        }
        private void Dispose()
        {
            m_Request.Dispose();
            m_RequestFinishedAction = null;
        }
    }
}
