thisdir = nunit20/framework
SUBDIRS = 
include ../../build/rules.make

LIBRARY = NUnit.Framework.dll
LIBRARY_NAME = nunit.framework.dll
LIBRARY_SNK = $(topdir)/nunit.key

LIB_MCS_FLAGS = -debug /r:System.Xml.dll /r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.framework.dll.csproj

include ../../build/library.make
