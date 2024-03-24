using System;

namespace Common
{
    public static class TimeUtils
    {
        private static readonly DateTime Now = DateTime.UtcNow;
        public static float GetTimeStamp()
        {
            var currentTimeUtc = DateTime.UtcNow;
            return (long)(currentTimeUtc - Now).TotalMilliseconds / 1000f;
        }
    }
    
    public class TimerAction
    {
        private Action m_Action;
        private readonly float m_ExpiredTime;

        public TimerAction(Action action, float delayTime)
        {
            m_Action = action;
            m_ExpiredTime = TimeUtils.GetTimeStamp() + delayTime;
            //Logger.LogInfo($"add delay call at {m_ExpiredTime}");
        }

        public bool IsExpired(float timeStamp)
        {
            return timeStamp >= m_ExpiredTime;
        }

        public void InvokeAction()
        {
            if (m_Action == null)
            {
                return;
            }
            //Logger.LogInfo($"invoke delay call at {TimeUtils.GetTimeStamp()}");
            m_Action.Invoke();
            m_Action = null;
        }
    }
}