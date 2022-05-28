using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Network.Ping
{
    class PingUtil
    {
        static private int PING_MAX_AVERAGER_COUNT = 5;
        static private int PING_MAX_VALUE = 1000;

        private Queue<float> m_PingAverager = new Queue<float>();
        private float m_PingTotal = 0;
        private float m_CurrentPing = 0;
        private float m_LastPingSentTime = 0;
        private bool m_IsLastPingBack = false;

        public float CurPing
        {
            get
            {
                return m_CurrentPing;
            }
        }
        public void PingSent(float sendTime)
        {
            if(m_IsLastPingBack == false && Mathf.Abs(m_LastPingSentTime) > Mathf.Epsilon)
            {
                AddPing(PING_MAX_VALUE);
            }
            m_LastPingSentTime = sendTime;
            m_IsLastPingBack = false;
        }
        public void PingBack(float lastSendTime)
        {
            if(m_IsLastPingBack == true)
            {
                return;
            }
            if (lastSendTime < m_LastPingSentTime)
            {
                return;
            }
            m_IsLastPingBack = true;
            AddPing((Time.time - lastSendTime) * 1000);
        }
        private void AddPing(float pingValue)
        {
            pingValue = Mathf.Clamp(pingValue, 0, PING_MAX_VALUE);
            if (m_PingAverager.Count >= PING_MAX_AVERAGER_COUNT)
            {
                m_PingTotal -= m_PingAverager.Dequeue();
            }
            m_PingAverager.Enqueue(pingValue);
            m_PingTotal += pingValue;
            m_CurrentPing = m_PingTotal / m_PingAverager.Count;
        }
    }
}
