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
        private void Init()
        {
            Console.CancelKeyPress += CancelKeyPressHandler;
            OnInit();
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
                var isRunning = OnRun();
                if (!isRunning)
                {
                    return;
                }
                UpdateTimerAction();
                //sleep for 16ms to simulate 60fps
                Thread.Sleep(16);
            }
            CleanedUp();
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
        
        private void UpdateTimerAction()
        {
            var curTimestamp = TimeUtils.GetTimeStamp();
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

        protected virtual bool OnRun()
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