thisdir = nunit24/ConsoleRunner/nunit-console
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit-console-runner.dll
KEYFILE = $(topdir)/nunit24/nunit.pub

LOCAL_MCS_FLAGS= /d:MONO /d:StronglyNamedAssembly /delaysign
LIB_REFS = nunit.core nunit.core.interfaces nunit.util System System.Xml
NO_TEST = yo

EXTRA_DISTFILES = nunit-console.csproj nunit-console_VS2005.csproj

include ../../../build/library.make
