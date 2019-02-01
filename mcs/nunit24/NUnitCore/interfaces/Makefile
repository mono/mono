thisdir = nunit24/NUnitCore/interfaces
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.interfaces.dll
KEYFILE = $(topdir)/nunit24/nunit.pub
NO_SIGN_ASSEMBLY = 1

ifdef MCS_MODE
KEYFILE = $(topdir)/class/mono.snk
NO_SIGN_ASSEMBLY =
endif

LOCAL_MCS_FLAGS= /d:StronglyNamedAssembly /publicsign
LIB_REFS = nunit.framework System
NO_TEST = yo

EXTRA_DISTFILES = nunit.core.interfaces.dll.csproj nunit.core.interfaces.dll_VS2005.csproj

include ../../../build/library.make
