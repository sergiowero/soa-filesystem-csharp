using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace FileSystem
{
    using Command = Tuple<Func<string, bool>, CommandType>;

    public enum CommandType
    {
        System,
        File,
        Both
    }

    public class FileSystem
    {
        private const string FILESYSTEM_FILE = "files.dat";
        private const string SETTINGS_FILE = "settings.txt";

        private bool inFileMode { get { return m_openedFileName != null; } }

        private Dictionary<string, Command> m_commands;
        private FileNode m_currentNode;
        private FileNode m_root;
        private Dictionary<string, string> m_settings;
        private MemoryStream m_openedFile;
        private string m_openedFileName;

        public FileSystem()
        {
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("File System for SOA");
            Console.WriteLine("---------------------------------------------------------");

            m_commands = new Dictionary<string, Command>();
            m_settings = new Dictionary<string, string>();
            m_openedFile = null;
            m_openedFileName = null;

            m_commands["quit"] = new Command(Exit, CommandType.Both);
            m_commands["exit"] = new Command(Exit, CommandType.Both);
            m_commands["mkdir"] = new Command(MakeDirectory, CommandType.System);
            m_commands["rmdir"] = new Command(RemoveDirectory, CommandType.System);
            m_commands["ls"] = new Command(List, CommandType.System);
            m_commands["cd"] = new Command(ChangeDir, CommandType.System);
            m_commands["find"] = new Command(FindNode, CommandType.System);
            m_commands["cls"] = new Command(Clear, CommandType.Both);
            m_commands[""] = new Command(None, CommandType.Both);
            m_commands["mk"] = new Command(CreateFile, CommandType.System);
            m_commands["rm"] = new Command(RemoveFile, CommandType.System);
            m_commands["open"] = new Command(OpenFile, CommandType.System);
            m_commands["close"] = new Command(CloseFile, CommandType.File);

            if (File.Exists(FILESYSTEM_FILE))
            {

                FileStream fs = new FileStream(FILESYSTEM_FILE, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    m_root = (FileNode)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    LogError("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }

            if (m_root == null)
            {
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
                SaveFileSystem();
            }
            m_currentNode = m_root;
            LoadSettingsFile();

            if (m_settings.ContainsKey("LastDir"))
            {
                m_currentNode = FindNode(m_settings["LastDir"], m_root);
            }
        }

        private void SaveFileSystem()
        {
            FileStream fs = new FileStream(FILESYSTEM_FILE, FileMode.OpenOrCreate);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, m_root);
            }
            catch (SerializationException e)
            {
                LogError("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        private void LoadSettingsFile()
        {
            if (!File.Exists(SETTINGS_FILE))
                return;

            m_settings.Clear();
            using (StreamReader reader = File.OpenText(SETTINGS_FILE))
            {
                string[] line = reader.ReadLine().Split('=');
                m_settings[line[0]] = line[1];
            }
        }

        private void SaveSettingsFile()
        {
            using (StreamWriter writer = File.CreateText(SETTINGS_FILE))
            {
                foreach (var item in m_settings)
                {
                    string value = string.Format("{0}={1}", item.Key, item.Value);
                    writer.WriteLine(value);
                }
            }
        }

        private void SaveAll()
        {
            SaveFileSystem();
            SaveSettingsFile();
        }

        public void Start()
        {
            bool check = true;
            while (check)
            {
                if (m_openedFileName != null)
                    Console.Write("File:" + m_openedFileName + " >> ");
                else
                    Console.Write(m_currentNode.absolutePath + " >> ");

                string command = Console.ReadLine();
                var tokens = command.Split(' ', '\t', '\n', '\r');
                if (m_commands.ContainsKey(tokens[0]))
                {
                    var param = tokens.Length > 1 ? tokens[1] : "";
                    Command cmd = m_commands[tokens[0]];
                    if (cmd.Item2 == CommandType.System && inFileMode)
                    {
                        LogInfo("Command \"{0}\" cannot be used when a file is open.\n", tokens[0]);
                    }
                    else if (cmd.Item2 == CommandType.File && !inFileMode)
                    {
                        LogInfo("Command \"{0}\" cannot be used without a open file.\n", tokens[0]);
                    }
                    else
                    {
                        check = cmd.Item1(param);
                    }
                    
                    Console.WriteLine();
                }
                else
                {
                    LogError("Command \"{0}\" not found.\n", tokens[0]);
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
                Console.WriteLine(string.Format(" {0,-30}{1}", node.Key, node.Value.PrintPermissions()));
            }
            return true;
        }

        private bool MakeDirectory(string _name)
        {
            var node = FindNode(_name, m_currentNode);

            if (node != null)
            {
                LogError("File or directory \"" + _name + "\" already exist.");
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
            SaveFileSystem();
            return true;
        }

        private bool RemoveDirectory(string _name)
        {
            var node = m_currentNode[_name];


            if (node == null)
            {
                LogError("Directory \"" + _name + "\" does not exist. You can only delete child directories.");
                return true;
            }

            if (node.type == FileNode.Type.File)
            {
                LogError("\"" + _name + "\" is not a directory.");
                return true;
            }

            m_currentNode.children.Remove(_name);

            //if(node == m_root)
            //{
            //    LogError("Imposible to remove root directory.");
            //    return true;
            //}

            //node[".."].children.Remove(node.relativePath);

            //if (node == m_currentNode)
            //{
            //    //TODO: Comprobar si el folder eliminado esta dentro es esta mas arriba de el grafo
            //    ChangeDir(node[".."].absolutePath);
            //}

            SaveFileSystem();
            return true;
        }

        private bool ChangeDir(string _name)
        {
            FileNode node = FindNode(_name, m_currentNode);
            if (node != null)
            {
                if (node.type == FileNode.Type.File)
                {
                    LogError("\"" + _name + "\" is not a directory.");
                }
                else
                {
                    m_currentNode = node;
                    m_settings["LastDir"] = m_currentNode.absolutePath;
                    SaveSettingsFile();
                }
            }
            else
            {
                LogError("\"" + _name + "\" not found.");
            }



            return true;
        }

        private bool CreateFile(string _name)
        {
            FileNode file = FindNode(_name, m_currentNode);
            if (file != null)
            {
                LogError("File \"{0}\" already exist.", _name);
                return true;
            }

            //Find parent node
            FileNode parent = null;
            string fileName = null;
            int index = _name.LastIndexOf('/');
            if (index == -1)
            {
                parent = m_currentNode;
                fileName = _name;
            }
            else
            {
                parent = FindNode(_name.Substring(0, index + 1), m_currentNode);
                fileName = _name.Substring(index + 1);
            }

            if (parent == null || string.IsNullOrEmpty(fileName) || parent.type == FileNode.Type.File)
            {
                LogError("Path \"{0}\" is invalid.", _name);
            }

            file = new FileNode()
            {
                absolutePath = parent.absolutePath + fileName,
                relativePath = fileName,
                permissions = FileNode.DEFAULT_PERMISSIONS,
                type = FileNode.Type.File,
                size = 0
            };

            parent[fileName] = file;
            SaveFileSystem();

            return true;
        }

        private bool RemoveFile(string _name)
        {
            FileNode file = FindNode(_name, m_currentNode);
            if (file == null)
            {
                LogError("File \"{0}\" does not exist.", _name);
                return true;
            }

            //Find parent node
            FileNode parent = null;
            string fileName = null;
            int index = _name.LastIndexOf('/');
            if (index == -1)
            {
                parent = m_currentNode;
                fileName = _name;
            }
            else
            {
                parent = FindNode(_name.Substring(0, index + 1), m_currentNode);
                fileName = _name.Substring(index + 1);
            }

            if (parent == null || string.IsNullOrEmpty(fileName) || parent.type == FileNode.Type.File)
            {
                LogError("Path \"{0}\" is invalid.", _name);
            }

            parent.children.Remove(fileName);
            SaveFileSystem();

            return true;
        }

        private bool OpenFile(string _name)
        {
            if (m_openedFile != null)
            {
                LogError("There is already a file previously opened ({0}). You cannot open two files at once.", m_openedFileName);
                return true;
            }

            FileNode file = FindNode(_name, m_currentNode);

            if (file == null)
            {
                LogError("File \"{0}\" does not exist", _name);
                return true;
            }

            if (file.type == FileNode.Type.Directory)
            {
                LogError("File \"{0}\" is a directory");
                return true;
            }

            m_openedFile = new MemoryStream();
            m_openedFileName = file.relativePath;

            return true;
        }

        private bool CloseFile(string _)
        {
            if (m_openedFile != null)
            {
                m_openedFile.Close();
                m_openedFile = null;
                m_openedFileName = null;
            }
            else
            {
                LogError("There is not a opened file. You need to open a file to close it!!!");
            }

            return true;
        }

        private void LogInfo(string _format, params object[] _params)
        {
            _format = " [INFO] " + _format;
            Console.WriteLine(string.Format(_format, _params));
        }

        private void LogError(string _format, params object[] _params)
        {
            _format = " [ERROR] " + _format;
            Console.WriteLine(string.Format(_format, _params));
        }
    }
}
