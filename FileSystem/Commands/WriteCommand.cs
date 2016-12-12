namespace FileSystem.Commands
{
    public class WriteCommand : Command
    {
        public override bool saveSystemAfterExecute { get { return true; } }

        public override bool Execute(params string[] _args)
        {
            if (_args.Length < 2)
            {
                SysLog.LogError("Not enought arguments");
                return true;
            }
            string name = _args[0];
            FileNode file = fileSystem.Open(name, false);

            if (file != null)
            {
                string data = string.Join(" ", _args, 1, _args.Length - 1);

                file.Write(data);

                fileSystem.Close(file);
            }

            return true;
        }
    }
}