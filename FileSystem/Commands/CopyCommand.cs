namespace FileSystem.Commands
{
    public class CopyCommand : Command
    {
        public override bool saveSystemAfterExecute { get { return true; } }

        public override bool Execute(params string[] _args)
        {
            if (_args.Length < 2)
            {
                SysLog.LogError("Insuficient arguments");
                return true;
            }
            return true;
        }
    }
}