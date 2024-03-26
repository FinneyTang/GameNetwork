using System;
using System.Collections.Generic;
using System.Threading;

namespace Common
{
    public abstract class AppBase
    {
        private bool m_HasInited = false;
        private bool m_HasCleanedUp = false;
        
        private uint m_TimerActionId = 0;
        private readonly Dictionary<uint, TimerAction> m_TimerActions = new Dictionary<uint, TimerAction>();

        private int m_TargetFPS;
        private float m_FrameDuration;

        protected AppBase()
        {
            SetTargetFPS(30); //set fps to 30
        }
        private void Init()
        {
            Console.CancelKeyPress += CancelKeyPressHandler;
            OnInit();
        }
        protected void SetTargetFPS(int fps)
        {
            m_TargetFPS = Math.Max(1, fps);
            m_FrameDuration = 1f / m_TargetFPS;
        }
        public void Run()
        {
            if (!m_HasInited)
            {
                Init();
                m_HasInited = true;
            }
            
            while (true)
            {
                var curTimestamp = TimeUtils.GetTimeStamp();
                
                var isRunning = OnRun(curTimestamp);
                if (!isRunning)
                {
                    CleanedUp();
                    return;
                }
                UpdateTimerAction(curTimestamp);
                
                //keep frame rate
                var elapsedTime = TimeUtils.GetTimeStamp() - curTimestamp;
                var remainingTime = m_FrameDuration - elapsedTime;
                if(remainingTime > 0)
                {
                    Thread.Sleep((int)(remainingTime * 1000));
                }
            }
        }
        private void CleanedUp()
        {
            if (m_HasCleanedUp)
            {
                return;
            }
            m_HasCleanedUp = true;
            OnCleanup();
        }

        private readonly List<uint> m_TimerActionToRemove = new List<uint>();
        protected uint DelayCall(Action action, float delay)
        {
            var timerActionId = m_TimerActionId;
            var timerAction = new TimerAction(action, delay);
            m_TimerActions.Add(timerActionId, timerAction);
            m_TimerActionId++;
            return timerActionId;
        }
        
        private void UpdateTimerAction(float curTimestamp)
        {
            foreach (var pair in m_TimerActions)
            {
                if (pair.Value.IsExpired(curTimestamp))
                {
                    m_TimerActionToRemove.Add(pair.Key);
                }
            }
            foreach (var timerActionId in m_TimerActionToRemove)
            {
                if (m_TimerActions.TryGetValue(timerActionId, out var timerAction))
                {
                    timerAction.InvokeAction();
                    m_TimerActions.Remove(timerActionId);
                }
            }
            m_TimerActionToRemove.Clear();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual bool OnRun(float curTimestamp)
        {
            return true;
        }

        protected virtual void OnCleanup()
        {
        }
        
        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            Logger.LogInfo("Ctrl+C pressed");
            e.Cancel = true;
            CleanedUp();
            Environment.Exit(0);
        }
    }
}