using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    [Serializable]
    public class FileNode : IComparable<FileNode>
    {
        public const short DEFAULT_PERMISSIONS = 0x01FF;

        public enum Type
        {
            Directory,
            File
        }

        public Type type;
        public string absolutePath;
        public string relativePath;
        public byte[] data;
        public long size;
        public short permissions;
        public SortedDictionary<string, FileNode> children;

        public FileNode()
        {
            children = new SortedDictionary<string, FileNode>();
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

        public override string ToString()
        {
            return absolutePath;
        }
    }
}
