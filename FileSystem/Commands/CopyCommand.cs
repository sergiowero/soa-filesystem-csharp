
using System;

namespace FileSystem.Commands
{
    public class CopyCommand : Command
    {
        public override bool saveSystemAfterExecute { get { return true; } }

        public override bool Execute(params string[] _args)
        {
            return true;
        }
    }
}
