thisdir = nunit24/NUnitMocks/mocks
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.mocks.dll
KEYFILE = $(topdir)/nunit24/nunit.pub

LOCAL_MCS_FLAGS= /d:StronglyNamedAssembly /delaysign
LIB_REFS = nunit.framework System
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.mocks.csproj \
	nunit.mocks_VS2005.csproj

include ../../../build/library.make
