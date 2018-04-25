thisdir = nunit24/ConsoleRunner/nunit-console
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit-console-runner.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= /d:MONO /d:StronglyNamedAssembly /publicsign /keyfile:../../nunit.snk
LIB_REFS = nunit.core nunit.core.interfaces nunit.util System System.Xml
NO_TEST = yo

EXTRA_DISTFILES = nunit-console.csproj nunit-console_VS2005.csproj

include ../../../build/library.make
