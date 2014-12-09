thisdir = nunit24/ConsoleRunner/nunit-console
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit-console-runner.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= \
	-r:nunit.core.dll -r:nunit.core.interfaces.dll -r:nunit.util.dll \
	-r:System.dll -r:System.Xml.dll \
	/d:MONO /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = nunit-console.csproj nunit-console_VS2005.csproj

include ../../../build/library.make
