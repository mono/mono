thisdir = nunit24/NUnitCore/core
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS = /d:StronglyNamedAssembly -warn:1 /publicsign
LIB_REFS = nunit.framework nunit.core.interfaces System
NO_TEST = yo

EXTRA_DISTFILES = nunit.core.dll.csproj nunit.core.dll_VS2005.csproj

include ../../../build/library.make
