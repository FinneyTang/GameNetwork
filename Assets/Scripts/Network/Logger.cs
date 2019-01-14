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
                    fmtWithColorTag = "<color=green>{0}</color>";
                    break;
                case LogColor.Yellow:
                    fmtWithColorTag = "<color=yellow>{0}</color>";
                    break;
                case LogColor.Red:
                    fmtWithColorTag = "<color=red>{0}</color>";
                    break;
                default:
                    fmtWithColorTag = "{0}";
                    break;
            }
            Debug.Log(string.Format(fmtWithColorTag, message));
        }
    }
}
