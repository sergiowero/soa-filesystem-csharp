namespace FileSystem.Commands
{
    public class ChangeDirectoryCommand : Command
    {
        public override bool saveSettingsAfterExecute { get { return true; } }

        public override bool Execute(params string[] _args)
        {
            if (_args.Length == 0)
            {
                SysLog.LogError("Not enought arguments");
                return true;
            }
            string name = _args[0];
            FileNode node = fileSystem.FindNode(name, fileSystem.currentNode);
            if (node != null)
            {
                if (node.type == FileNode.Type.File)
                {
                    SysLog.LogError("\"" + name + "\" is not a directory.");
                }
                else
                {
                    fileSystem.currentNode = node;
                    fileSystem.settings["LastDir"] = fileSystem.currentNode.absolutePath;
                }
            }
            else
            {
                SysLog.LogError("\"" + name + "\" not found.");
            }
            return true;
        }
    }
}