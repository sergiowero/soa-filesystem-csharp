using System;

namespace FileSystem.Devices
{
    [Serializable]
    public class ZeroDevice : FileNode
    {

        public ZeroDevice() :base(Type.Device)
        {
        }

        public override void Open()
        {
            
        }

        public override string ReadAll()
        {
            return "0";
        }

        public override void Read(byte[] _buff, int offset, int count)
        {
            for(int i = offset, c = 0 ; i < _buff.Length && c < count ; i++, c++)
            {
                _buff[i] = 0;
            }
        }

        public override void Write(string _data)
        {
            
        }
    }
}
