using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileSystem.Devices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [Serializable]
    public class MouseDevice : FileNode
    {
#if WINDOWS

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

#endif

#if LINUX
        private void GetCursorPos(out POINT lpPoint)
        {
            lpPoint = new POINT();

            Process proc = new Process();
            proc.StartInfo.FileName = "xdotool";
            proc.StartInfo.Arguments = "getmouselocation";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            // output example
            // x:852 y:689 screen:0 window:37748742
            Regex r = new Regex(@"^x:(?<x>\d+)\sy:(?<y>\d+)", RegexOptions.None);
            Match m = r.Match(output);
            if (m.Success)
            {
                lpPoint.X = int.Parse(m.Result("${x}"));
                lpPoint.Y = int.Parse(m.Result("${y}"));
            }
        }
#endif

        public MouseDevice() : base(Type.Device)
        {
        }

        public override void Open()
        {
        }

        public override string ReadAll()
        {
            POINT point;
            GetCursorPos(out point);

            return string.Format("{0},{1}", point.X, point.Y);
        }

        public override void Read(byte[] _buff, int offset, int count)
        {
            POINT point;
            GetCursorPos(out point);

            memoryHandle = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryHandle);
            writer.Write(point.X);
            writer.Write(point.Y);
            writer.Close();
            memoryHandle.Flush();
            byte[] source = memoryHandle.ToArray();
            memoryHandle = null;

            for (int i = offset, c = 0 ; i < _buff.Length && c < count && c < source.Length ; i++, c++)
            {
                _buff[i] = source[c];
            }
        }

        public override void Write(string _data)
        {
        }
    }
}