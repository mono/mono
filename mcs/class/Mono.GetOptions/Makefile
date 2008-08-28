thisdir = class/Mono.GetOptions
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.GetOptions.dll
LIB_MCS_FLAGS = /r:System.dll -nowarn:618
NO_TEST = yes

EXTRA_DISTFILES = \
	Mono.GetOptions.csproj		\
	GetOptTest/AssemblyInfo.cs	\
	GetOptTest/GetOptTester.cs \
	Samples/Makefile \
	Samples/mcat.exe.sources \
	Samples/mcat.cs

include ../../build/library.make
