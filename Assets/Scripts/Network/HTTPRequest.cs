﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network.HTTP
{
    class HTTPRequest
    {
        const string ERR_InvalidArguments = "invalid arguments";
        const string ERR_Timeout = "timeout";

        const float DEFAULT_TIMEOUT = 5;

        private UnityWebRequest m_Request;
        private Action<bool, byte[]> m_RequestFinishedAction;
        private float m_Timeout;
        private string m_LastError = string.Empty;
        private float m_LastProgress = 0f;
        public string LastError => m_LastError;

        public float Progress
        {
            get
            {
                if (m_Request != null)
                {
                    return m_Request.downloadProgress;
                }
                return 0f;
            }
        }
        public void Get(string addr, string cmd, Action<bool, byte[]> onRequestFinished, params object[] args)
        {
            if (args.Length % 2 != 0)
            {
                m_LastError = ERR_InvalidArguments;
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
                paramStr += (args[i * 2] + "=" + Uri.EscapeDataString(args[i * 2 + 1].ToString()));
            }
            var url = Uri.EscapeUriString(addr) + cmd + paramStr;
            m_RequestFinishedAction = onRequestFinished;
            m_Timeout = Time.time + DEFAULT_TIMEOUT;
            m_Request = new UnityWebRequest(url);
            m_Request.downloadHandler = new DownloadHandlerBuffer();
            m_Request.SendWebRequest();
        }
        public void Post(string addr, string cmd, Action<bool, byte[]> onRequestFinished, byte[] data, Dictionary<string, string> header)
        {
            string url = Uri.EscapeUriString(addr) + cmd;
            m_RequestFinishedAction = onRequestFinished;
            if(data != null && data.Length == 0)
            {
                data = null;
            }
            m_Timeout = Time.time + DEFAULT_TIMEOUT;
            m_Request = new UnityWebRequest(url, "POST");
            m_Request.uploadHandler = new UploadHandlerRaw(data);
            m_Request.downloadHandler = new DownloadHandlerBuffer();
            if(header != null)
            {
                foreach(var pair in header)
                {
                    m_Request.SetRequestHeader(pair.Key, pair.Value);
                }
            }
            m_Request.SendWebRequest();
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
                m_RequestFinishedAction.Invoke(string.IsNullOrEmpty(m_LastError), m_Request.downloadHandler.data);
                Dispose();
            }
            else
            {
                if(m_Request.downloadProgress > m_LastProgress)
                {
                    m_Timeout = Time.time + DEFAULT_TIMEOUT;
                    m_LastProgress = m_Request.downloadProgress;
                }
                if(Time.time > m_Timeout)
                {
                    m_LastError = ERR_Timeout;
                    m_RequestFinishedAction.Invoke(false, null);
                    Dispose();
                }
            }
        }
        private void Dispose()
        {
            m_Request.Dispose();
            m_Request = null;
            m_RequestFinishedAction = null;
        }
    }
}
