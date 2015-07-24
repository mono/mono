thisdir = nunit20/NUnitCore/core
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= -r:nunit.framework.dll -r:nunit.core.interfaces.dll -r:System.dll /d:StronglyNamedAssembly -warn:1
NO_TEST = yo

EXTRA_DISTFILES = nunit.core.dll.csproj nunit.core.dll_VS2005.csproj

include ../../../build/library.make
