thisdir = nunit24/NUnitFramework/framework
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = NUnit.Framework.dll
LIBRARY_NAME = nunit.framework.dll

KEYFILE = $(topdir)/nunit24/nunit.pub

LIB_MCS_FLAGS = /d:StronglyNamedAssembly -warn:1 /delaysign
LIB_REFS = System.Xml System
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.framework.dll.csproj \
	nunit.framework.dll_VS2005.csproj

include ../../../build/library.make
