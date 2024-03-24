using System;
using UnityEngine;

namespace Network.Core
{
    public static class ColoredLogger
    {
        public enum LogColor
        {
            None, Green, Yellow, Red
        }
        public static void Log(object message, LogColor type = LogColor.None)
        {
            string fmtWithColorTag;
            switch (type)
            {
                case LogColor.Green:
                    fmtWithColorTag = "[{0}]: <color=green>{1}</color>";
                    break;
                case LogColor.Yellow:
                    fmtWithColorTag = "[{0}]: <color=yellow>{1}</color>";
                    break;
                case LogColor.Red:
                    fmtWithColorTag = "[{0}]: <color=red>{1}</color>";
                    break;
                default:
                    fmtWithColorTag = "[{0}]: {1}";
                    break;
            }
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.Log(string.Format(fmtWithColorTag, timestamp, message));
        }
    }
}
