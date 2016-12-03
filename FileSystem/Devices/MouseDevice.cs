using System;
using System.Runtime.InteropServices;
using System.IO;

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

    public class MouseDevice : FileNode
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        public MouseDevice() :base(Type.Device)
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

            for (int i = offset, c = 0 ; i < _buff.Length && c < count && c < source.Length; i++, c++)
            {
                _buff[i] = source[c];
            }
        }

        public override void Write(string _data)
        {

        }
    }
}
