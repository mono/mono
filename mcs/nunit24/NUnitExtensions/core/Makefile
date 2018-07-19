thisdir = nunit24/NUnitExtensions/core
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.extensions.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LIB_MCS_FLAGS =  /d:StronglyNamedAssembly /publicsign /keyfile:../../nunit.snk
LIB_REFS = nunit.core nunit.core.interfaces System.Xml System
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.core.extensions.dll.csproj \
	nunit.core.extensions.dll_VS2005.csproj

include ../../../build/library.make
