﻿using System;

namespace FileSystem.Commands
{
    public class ListCommand : Command
    {
        public override bool Execute(params string[] _args)
        {
            foreach (var node in fileSystem.currentNode.children)
            {
                Console.WriteLine(string.Format(" {0,-30}{1}", node.Key, node.Value.PrintPermissions()));
            }
            return true;
        }
    }
}