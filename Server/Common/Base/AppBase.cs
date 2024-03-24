using System;
using System.Threading;

namespace Common
{
    public abstract class AppBase
    {
        private bool m_HasInited = false;
        private bool m_HasCleanedUp = false;
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
            var isRunning = true;
            while (isRunning)
            {
                isRunning = OnRun();
                
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