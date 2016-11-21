namespace FileSystem.Commands
{
    public class EmptyCommand : Command
    {
        public override bool Execute(params string[] _args)
        {
            return true;
        }
    }
}