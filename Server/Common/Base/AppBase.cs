using System;
using System.Threading;

namespace Common
{
    public abstract class AppBase
    {
        public void Run()
        {
            Console.CancelKeyPress += CancelKeyPressHandler;
            OnRun();
            //wait until Ctrl+C
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        protected abstract void OnRun();
        protected abstract void OnCleanup();
        private void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            Logger.LogInfo("Ctrl+C pressed");
            e.Cancel = true;
            OnCleanup();
            Environment.Exit(0);
        }
    }
}