using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Commands
{
    public class MakeDeviceCommand : Command
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
            string type = _args[1];

            fileSystem.CreateDeviceFile(name, type);
            return true;
        }
    }
}
