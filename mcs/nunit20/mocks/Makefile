thisdir = nunit20/mocks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = nunit.mocks.dll
LIBRARY_SNK = $(topdir)/nunit.key

LOCAL_MCS_FLAGS= -debug -r:nunit.framework.dll -r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

ifeq (net_2_0, $(PROFILE))
NO_INSTALL = yes
endif


EXTRA_DISTFILES = nunit.mocks.csproj

include ../../build/library.make
