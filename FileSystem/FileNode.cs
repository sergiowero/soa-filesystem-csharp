using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSystem
{
    [Serializable]
    public class FileNode : IComparable<FileNode>, IEnumerable<FileNode>
    {
        public const short DEFAULT_PERMISSIONS = 0x01E4;

        public enum Type
        {
            Directory,
            File,
            Device
        }

        public Type type { get; private set; }
        public string absolutePath;
        public string relativePath;
        public byte[] data { get; private set; }
        public long size { get { return data != null ? data.Length : 0; } }
        public short permissions;
        public SortedDictionary<string, FileNode> children;
        public long creationTime;
        public long modificationTime;
        protected MemoryStream memoryHandle;

        public bool isOpen { get { return memoryHandle != null; } }

        public FileNode(Type _type)
        {
            children = new SortedDictionary<string, FileNode>();
            type = _type;
        }

        public FileNode this[string key]
        {
            get
            {
                FileNode node = null;
                children.TryGetValue(key, out node);
                return node;
            }
            set
            {
                children[key] = value;
            }
        }

        public void SetPemissions(byte _user, byte _group, byte _all)
        {
            byte u = (byte)(0x07 & _user);
            byte g = (byte)(0x07 & _group);
            byte a = (byte)(0x07 & _all);

            permissions = (short)((u << 6) | (g << 3) | (a));
        }

        public int CompareTo(FileNode other)
        {
            return string.Compare(this.relativePath, other.relativePath);
        }

        public string PrintPermissions()
        {
            byte u = (byte)(0x07 & (permissions >> 6));
            byte g = (byte)(0x07 & (permissions >> 3));
            byte a = (byte)(0x07 & (permissions >> 0));
            return string.Format("{0}{1}{2}{3}", type == Type.Directory ? "d" : "-", Permit(u), Permit(g), Permit(a));
        }

        private string Permit(byte _value)
        {
            string permit = (_value & 0x4) != 0 ? "r" : "-";
            permit += (_value & 0x2) != 0 ? "w" : "-";
            permit += (_value & 0x1) != 0 ? 'x' : '-';
            return permit;
        }

        public string PrintTime()
        {
            string creation = DateTime.FromBinary(creationTime).ToString("dd/MM/yyyy hh:mm");
            string modification = DateTime.FromBinary(modificationTime).ToString("dd/MM/yyyy hh:mm");

            return string.Format("c: {0:-15} m: {1:-15}", creation, modification);
        }

        public override string ToString()
        {
            return absolutePath;
        }

        public IEnumerator<FileNode> GetEnumerator()
        {
            foreach (var item in children) yield return item.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return children.GetEnumerator();
        }

        public virtual void Open()
        {
            data = data ?? new byte[0];
            memoryHandle = new MemoryStream();
            memoryHandle.Write(data, 0, data.Length);
            modificationTime = DateTime.Now.ToBinary();
        }

        public virtual void Write(string _data)
        {
            if (memoryHandle != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(_data);
                memoryHandle.Write(data, 0, data.Length);
            }
        }

        public virtual string ReadAll()
        {
            if (memoryHandle != null)
            {
                return Encoding.UTF8.GetString(memoryHandle.ToArray());
            }
            else
            {
                return "";
            }
        }

        public virtual void Read(byte[] _buff, int offset, int count)
        {
            if(memoryHandle != null && _buff != null)
            {
                memoryHandle.Read(_buff, offset, count);
            }
        }

        public void Write(byte[] _data)
        {
            if (memoryHandle != null)
            {
                memoryHandle.Write(_data, 0, _data.Length);
            }
        }

        public void Flush()
        {
            if (memoryHandle != null)
            {
                memoryHandle.Flush();
                data = memoryHandle.ToArray();
            }
        }

        public void Close()
        {
            Flush();
            memoryHandle = null;
        }
    }
}