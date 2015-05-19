thisdir = nunit24/NUnitMocks/mocks
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.mocks.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= -debug -r:nunit.framework.dll -r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.mocks.csproj \
	nunit.mocks_VS2005.csproj

include ../../../build/library.make
