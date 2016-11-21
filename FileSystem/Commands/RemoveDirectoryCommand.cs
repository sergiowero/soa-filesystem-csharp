namespace FileSystem.Commands
{
    public class RemoveDirectoryCommand : Command
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
            var node = fileSystem.currentNode[name];

            if (node == null)
            {
                SysLog.LogError("Directory \"" + name + "\" does not exist. You can only delete child directories.");
                return true;
            }

            if (node.type == FileNode.Type.File)
            {
                SysLog.LogError("\"" + name + "\" is not a directory.");
                return true;
            }

            fileSystem.currentNode.children.Remove(name);

            //if(node == m_root)
            //{
            //    SysLog.LogError("Imposible to remove root directory.");
            //    return true;
            //}

            //node[".."].children.Remove(node.relativePath);

            //if (node == m_currentNode)
            //{
            //    //TODO: Comprobar si el folder eliminado esta dentro es esta mas arriba de el grafo
            //    ChangeDir(node[".."].absolutePath);
            //}

            return true;
        }
    }
}