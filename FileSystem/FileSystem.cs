﻿using FileSystem.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    public class FileSystem
    {
        private const string FILESYSTEM_FILE = "files.dat";
        private const string SETTINGS_FILE = "settings.txt";

        private bool inFileMode { get { return m_openedFileName != null; } }

        private Dictionary<string, Command> m_commands;
        public FileNode currentNode;
        private FileNode m_root;
        public Dictionary<string, string> settings;
        private MemoryStream m_openedFile;
        private string m_openedFileName;

        public FileSystem()
        {
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("File System for SOA");
            Console.WriteLine("---------------------------------------------------------");

            m_commands = new Dictionary<string, Command>();
            settings = new Dictionary<string, string>();
            m_openedFile = null;
            m_openedFileName = null;

            m_commands["quit"] = new ExitCommand();
            m_commands["exit"] = new ExitCommand();
            m_commands[""] = new EmptyCommand();
            m_commands["mkdir"] = new MakeDirectoryCommand();
            m_commands["rmdir"] = new RemoveDirectoryCommand();
            m_commands["ls"] = new ListCommand();
            m_commands["cd"] = new ChangeDirectoryCommand();
            //m_commands["find"] = new Command(FindNode, CommandType.System);
            m_commands["cls"] = new ClearCommand();

            m_commands["touch"] = new TouchCommand();
            m_commands["rm"] = new RemoveCommand();
            //m_commands["open"] = new Command(OpenFile, CommandType.System);
            //m_commands["close"] = new Command(CloseFile, CommandType.File);

            foreach (var cmd in m_commands)
            {
                cmd.Value.fileSystem = this;
            }

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
                    SysLog.LogError("Failed to deserialize. Reason: " + e.Message);
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
            currentNode = m_root;
            LoadSettingsFile();

            if (settings.ContainsKey("LastDir"))
            {
                currentNode = FindNode(settings["LastDir"], m_root);
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
                SysLog.LogError("Failed to serialize. Reason: " + e.Message);
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

            settings.Clear();
            using (StreamReader reader = File.OpenText(SETTINGS_FILE))
            {
                string[] line = reader.ReadLine().Split('=');
                settings[line[0]] = line[1];
            }
        }

        private void SaveSettingsFile()
        {
            using (StreamWriter writer = File.CreateText(SETTINGS_FILE))
            {
                foreach (var item in settings)
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
                    Console.Write(currentNode.absolutePath + " >> ");

                string command = Console.ReadLine();
                var tokens = command.Split(' ', '\t', '\n', '\r');
                if (m_commands.ContainsKey(tokens[0]))
                {
                    string[] args = new string[0];
                    if (tokens.Length > 1)
                    {
                        args = new string[tokens.Length - 1];
                        Array.Copy(tokens, 1, args, 0, args.Length);
                    }

                    Command cmd = m_commands[tokens[0]];
                    check = cmd.Execute(args);
                    Console.WriteLine();

                    if (cmd.saveSystemAfterExecute)
                    {
                        SaveFileSystem();
                    }
                    if (cmd.saveSettingsAfterExecute)
                    {
                        SaveSettingsFile();
                    }
                }
                else
                {
                    SysLog.LogError("Command \"{0}\" not found.\n", tokens[0]);
                }
            }
        }

        public bool FindNode(string _name)
        {
            bool res = FindNode(_name, currentNode) != null;
            Console.WriteLine(" Found => " + res);

            return res;
        }

        public FileNode FindNode(string _name, FileNode _current = null)
        {
            FileNode result = null;

            if (_current == null)
                _current = currentNode;

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

        private bool OpenFile(string _name)
        {
            if (m_openedFile != null)
            {
                SysLog.LogError("There is already a file previously opened ({0}). You cannot open two files at once.", m_openedFileName);
                return true;
            }

            FileNode file = FindNode(_name, currentNode);

            if (file == null)
            {
                SysLog.LogError("File \"{0}\" does not exist", _name);
                return true;
            }

            if (file.type == FileNode.Type.Directory)
            {
                SysLog.LogError("File \"{0}\" is a directory");
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
                SysLog.LogError("There is not a opened file. You need to open a file to close it!!!");
            }

            return true;
        }
    }
}