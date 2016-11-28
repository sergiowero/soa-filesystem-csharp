using System;

namespace FileSystem.Commands
{
    public class MakeDirectoryCommand : Command
    {
        public override bool saveSystemAfterExecute { get { return true; } }

        public override bool Execute(params string[] _args)
        {
            if (_args.Length == 0)
            {
                SysLog.LogError("Not enought arguments");
                return true;
            }
            string name = _args[0];
            var node = fileSystem.FindNode(name, fileSystem.currentNode);

            if (node != null)
            {
                SysLog.LogError("File or directory \"" + name + "\" already exist.");
                return true;
            }

            var newDir = new FileNode()
            {
                absolutePath = fileSystem.currentNode.absolutePath + name + "/",
                relativePath = name,
                permissions = FileNode.DEFAULT_PERMISSIONS,
                type = FileNode.Type.Directory,
                creationTime = DateTime.Now.ToBinary(),
                modificationTime = DateTime.Now.ToBinary()
            };

            newDir["."] = newDir;
            newDir[".."] = fileSystem.currentNode;
            fileSystem.currentNode[name] = newDir;
            return true;
        }
    }
}