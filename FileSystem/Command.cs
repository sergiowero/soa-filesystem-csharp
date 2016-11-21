namespace FileSystem
{
    public abstract class Command
    {
        public FileSystem fileSystem;

        public virtual bool saveSystemAfterExecute { get { return false; } }
        public virtual bool saveSettingsAfterExecute { get { return false; } }

        public abstract bool Execute(params string[] _args);
    }
}