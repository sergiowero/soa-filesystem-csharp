using System;

namespace FileSystem
{
    public enum CommandType
    {
        System,
        File,
        Both
    }

    public delegate bool CommandDelegate(string _param);

    public class Command
    {

        public CommandDelegate callback { get; private set; }
        public CommandType type { get; private set; }

        public Command(CommandDelegate _callback, CommandType _type)
        {
            callback = _callback;
            type = _type;
        }
    }
}