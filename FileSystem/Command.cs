using System;

namespace FileSystem
{
    public enum CommandType
    {
        System,
        File,
        Both
    }

    public class Command
    {
        public Func<string, bool> callback { get; private set; }
        public CommandType type { get; private set; }

        public Command(Func<string, bool> _callback, CommandType _type)
        {
            callback = _callback;
            type = _type;
        }
    }
}