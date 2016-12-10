all: FileSystem


SOURCES=Command.cs FileNode.cs FileSystem.cs Program.cs SysLog.cs $(wildcard Commands/*.cs) $(wildcard Devices/*.cs) Properties/AssemblyInfo.cs

lib: 
	mkdir lib

FileSystem: $(SOURCES)  lib
	mcs $(SOURCES) -out:lib/FileSystem -define:LINUX
