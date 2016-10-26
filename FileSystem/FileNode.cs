using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    [Serializable]
    public class FileNode : IComparable<FileNode>, IEnumerable<FileNode>
    {
        public const short DEFAULT_PERMISSIONS = 0x01E4;

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

        public string PrintPermissions()
        {
            byte u = (byte)(0x07 & (permissions >> 6));
            byte g = (byte)(0x07 & (permissions >> 3));
            byte a = (byte)(0x07 & (permissions >> 0));
            return string.Format("{0}{1}{2}{3}", type == Type.Directory ? "d": "-", Permit(u), Permit(g), Permit(a));
        }

        private string Permit(byte _value)
        {
            string permit = (_value & 0x4) != 0 ? "r" : "-";
            permit       += (_value & 0x2) != 0 ? "w" : "-";
            permit       += (_value & 0x1) != 0 ? 'x' : '-';
            return permit;
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
    }
}
