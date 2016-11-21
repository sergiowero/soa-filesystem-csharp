using System;

namespace FileSystem
{
    public static class SysLog
    {
        public static void LogInfo(string _format, params object[] _params)
        {
            _format = " [INFO] " + _format;
            Console.WriteLine(string.Format(_format, _params));
        }

        public static void LogError(string _format, params object[] _params)
        {
            _format = " [ERROR] " + _format;
            Console.WriteLine(string.Format(_format, _params));
        }
    }
}