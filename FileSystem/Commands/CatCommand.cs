using System;

namespace FileSystem.Commands
{
    public class CatCommand : Command
    {
        public override bool saveSettingsAfterExecute { get { return false; } }

        public override bool Execute(params string[] _args)
        {
            if (_args.Length == 0)
            {
                SysLog.LogError("Not enought arguments");
                return true;
            }
            string name = _args[0];

            FileNode file = fileSystem.Open(name, false);
            if (file != null)
            {
                Console.WriteLine();
                Console.WriteLine(file.ReadAll());
                fileSystem.Close(file);
            }

            return true;
        }
    }
}