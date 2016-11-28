using System;

namespace FileSystem.Commands
{
    public class TouchCommand : Command
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

            FileNode file = fileSystem.Open(name);
            fileSystem.Close(file);
            return true;
        }
    }
}