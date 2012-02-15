thisdir = class/Mono.CSharp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CSharp.dll

LIB_MCS_FLAGS = -r:System.Core.dll -r:System.Xml -r:System

Mono.CSharp.dll.sources: ../../mcs/gmcs.exe.sources Makefile
	cat ../../mcs/gmcs.exe.sources | grep -v AssemblyInfo.cs | grep -v doc-bootstrap.cs | sed -e 's,^,../../mcs/,' -e 's,mcs/../,,' > Mono.CSharp.dll.sources
	echo "../../mcs/cs-parser.cs" >> Mono.CSharp.dll.sources
	echo "Assembly/AssemblyInfo.cs" >> Mono.CSharp.dll.sources

include ../../build/library.make

