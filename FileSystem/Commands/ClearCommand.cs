using System;

namespace FileSystem.Commands
{
    public class ClearCommand : Command
    {
        public override bool Execute(params string[] _args)
        {
            Console.Clear();
            return true;
        }
    }
}