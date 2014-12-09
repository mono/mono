thisdir = nunit24/NUnitCore/interfaces
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.interfaces.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= -debug -r:nunit.framework.dll -r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = nunit.core.interfaces.dll.csproj nunit.core.interfaces.dll_VS2005.csproj

include ../../../build/library.make
