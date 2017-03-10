thisdir = nunit24/NUnitFramework/framework
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = NUnit.Framework.dll
LIBRARY_NAME = nunit.framework.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LIB_MCS_FLAGS = /d:StronglyNamedAssembly -warn:1 /publicsign /keyfile:../../nunit.snk
LIB_REFS = System.Xml System
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.framework.dll.csproj \
	nunit.framework.dll_VS2005.csproj

include ../../../build/library.make
