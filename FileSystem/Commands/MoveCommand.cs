namespace FileSystem.Commands
{
    public class MoveCommand : Command
    {
        public override bool saveSystemAfterExecute { get { return true; } }

        public override bool Execute(params string[] _args)
        {
            if (_args.Length < 2)
            {
                SysLog.LogError("Insuficient arguments");
                return true;
            }

            string source = _args[0];
            string destin = _args[1];

            FileNode sourceFile = fileSystem.FindNode(source);
            FileNode destinFile = fileSystem.FindNode(destin);

            if (sourceFile == null)
            {
                SysLog.LogError("Source file {0} not found.", source);
                return true;
            }

            if (destinFile != null)
            {
                SysLog.LogError("Destination file {0} already exist.", destin);
                return true;
            }

            destinFile = fileSystem.Open(destin);
            destinFile.Write(sourceFile.data);
            fileSystem.Close(destinFile);
            fileSystem.Remove(source);

            return true;
        }
    }
}