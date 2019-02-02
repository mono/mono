thisdir = nunit24/NUnitCore/interfaces
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.interfaces.dll
KEYFILE = $(topdir)/nunit24/nunit.pub

LOCAL_MCS_FLAGS= /d:StronglyNamedAssembly /delaysign
LIB_REFS = nunit.framework System
NO_TEST = yo

EXTRA_DISTFILES = nunit.core.interfaces.dll.csproj nunit.core.interfaces.dll_VS2005.csproj

include ../../../build/library.make
