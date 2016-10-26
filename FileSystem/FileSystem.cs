using System;
using System.IO;
using System.Collections.Generic;

namespace FileSystem
{
    public class FileSystem
    {
        private Dictionary<string, Func<string, bool>> m_commands;
        private FileNode m_currentNode;
        private FileNode m_root;

        public FileSystem()
        {
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("File System for SOA");
            Console.WriteLine("---------------------------------------------------------");

            m_commands = new Dictionary<string, Func<string, bool>>();

            m_commands["quit"] = Exit;
            m_commands["exit"] = Exit;
            m_commands["mkdir"] = MakeDirectory;
            m_commands["ls"] = List;
            m_commands["cd"] = ChangeDir;
            m_commands["find"] = FindNode;
            m_commands["cls"] = Clear;
            m_commands[""] = None;

            m_root = new FileNode()
            {
                absolutePath = "/",
                relativePath = "/",
                permissions = FileNode.DEFAULT_PERMISSIONS,
                type = FileNode.Type.Directory,
                size = 0
            };
            m_root["."] = m_root;
            m_root[".."] = m_root;
            m_currentNode = m_root;
        }

        public void Start()
        {
            bool check = true;
            while (check)
            {
                Console.Write(m_currentNode.absolutePath + " >> ");
                string command = Console.ReadLine();
                var tokens = command.Split(' ', '\t', '\n', '\r');
                if (m_commands.ContainsKey(tokens[0]))
                {
                    var param = tokens.Length > 1 ? tokens[1] : "";
                    check = m_commands[tokens[0]](param);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(string.Format(" [ERROR] Command \"{0}\" not found.\n", tokens[0]));
                }
            }
        }

        private bool FindNode(string _name)
        {
            bool res = FindNode(_name, m_currentNode) != null;
            Console.WriteLine(" Found => " + res);

            return res;
        }

        private FileNode FindNode(string _name, FileNode _current = null)
        {
            FileNode result = null;

            if (_current == null)
                _current = m_currentNode;

            if (_name.StartsWith("/"))
            {
                return FindNode(_name.Substring(1), m_root);
            }

            if (_name == "")
                return _current;

            int index = _name.IndexOf('/');

            string token = _name;
            if (index > -1)
            {
                token = _name.Substring(0, index);
                _name = _name.Substring(index + 1);
            }
            else
            {
                _name = "";
            }

            result = _current[token];

            if (result == null)
            {
                return result;
            }
            else if (result.type == FileNode.Type.Directory)
            {
                return FindNode(_name, result);
            }

            return result;
        }

        private bool Exit(string _)
        {
            return false;
        }

        private bool None(string _)
        {
            return true;
        }

        private bool Clear(string _)
        {
            Console.Clear();
            return true;
        }

        private bool List(string _)
        {
            foreach (var node in m_currentNode.children)
            {
                Console.WriteLine(" " + node.Key);
            }
            return true;
        }

        private bool MakeDirectory(string _name)
        {
            var node = m_currentNode[_name];

            if (node != null)
            {
                Console.WriteLine(" [ERROR] File or directory \"" + _name + "\" already exist.");
                return true;
            }

            var newDir = new FileNode()
            {
                absolutePath = m_currentNode.absolutePath + _name + "/",
                relativePath = _name,
                permissions = FileNode.DEFAULT_PERMISSIONS,
                type = FileNode.Type.Directory,
                size = 0
            };

            newDir["."] = newDir;
            newDir[".."] = m_currentNode;
            m_currentNode[_name] = newDir;
            return true;
        }

        private bool ChangeDir(string _name)
        {
            FileNode node = FindNode(_name, m_currentNode);
            if (node != null)
            {
                if (node.type == FileNode.Type.File)
                {
                    Console.WriteLine(" [ERROR] \"" + _name + "\" is not a directory.");
                }
                else
                {
                    m_currentNode = node;
                }
            }
            else
            {
                Console.WriteLine(" [ERROR] \"" + _name + "\" not found.");
            }

            return true;
        }

    }
}
