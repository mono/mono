thisdir = class/Mono.CSharp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CSharp.dll

docs = \
	en/ns-Mono.CSharp.xml				\
	en/index.xml					\
	en/Mono.CSharp/Evaluator.xml			\
	en/Mono.CSharp/Evaluator+NoValueSet.xml		\
	en/Mono.CSharp/InteractiveBase.xml		\
	en/Mono.CSharp/Report.xml			\
	en/Mono.CSharp/CompiledMethod.xml		\
	en/Mono.CSharp/InteractiveBase+Simple.xml

EXTRA_DISTFILES = $(docs)

LIB_MCS_FLAGS = -r:System.Core.dll -r:System.Xml -r:System

Mono.CSharp.dll.sources: ../../mcs/gmcs.exe.sources Makefile
	cat ../../mcs/gmcs.exe.sources | grep -v AssemblyInfo.cs  | sed -e 's,^,../../mcs/,' -e 's,mcs/../,,' > Mono.CSharp.dll.sources
	echo "../../mcs/cs-parser.cs" >> Mono.CSharp.dll.sources
	echo "Assembly/AssemblyInfo.cs" >> Mono.CSharp.dll.sources

include ../../build/library.make

