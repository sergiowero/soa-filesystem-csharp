namespace FileSystem.Commands
{
    public class RemoveCommand : Command
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

            FileNode file = fileSystem.FindNode(name, fileSystem.currentNode);
            if (file == null)
            {
                SysLog.LogError("File \"{0}\" does not exist.", name);
                return true;
            }

            //Find parent node
            FileNode parent = null;
            string fileName = null;
            int index = name.LastIndexOf('/');
            if (index == -1)
            {
                parent = fileSystem.currentNode;
                fileName = name;
            }
            else
            {
                parent   = fileSystem.FindNode(name.Substring(0, index + 1), fileSystem.currentNode);
                fileName = name.Substring(index + 1);
            }

            if (parent == null || string.IsNullOrEmpty(fileName) || parent.type == FileNode.Type.File)
            {
                SysLog.LogError("Path \"{0}\" is invalid.", name);
            }

            parent.children.Remove(fileName);
            return true;
        }
    }
}