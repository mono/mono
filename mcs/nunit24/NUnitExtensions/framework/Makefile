thisdir = nunit24/NUnitExtensions/framework
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.framework.extensions.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LIB_MCS_FLAGS = -debug /r:System.Xml.dll /r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.framework.extensions.dll.csproj \
	nunit.framework.extensions.dll_VS2005.csproj

include ../../../build/library.make
