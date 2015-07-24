thisdir = nunit24/ClientUtilities/util
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.util.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= \
	/resource:Transform.resources,NUnit.Util.Transform.resources \
	-r:nunit.core.dll -r:nunit.core.interfaces.dll -r:System.dll \
	-r:System.Xml.dll -r:System.Runtime.Remoting.dll \
	/d:MONO /d:StronglyNamedAssembly -warn:1
NO_TEST = yo

RESX_RES = Transform.resources

EXTRA_DISTFILES = \
	nunit.util.dll.csproj	\
	nunit.util.dll_VS2005.csproj	\
	$(RESX_RES:.resources=.resx)

CLEAN_FILES = $(RESX_RES)

include ../../../build/library.make

$(the_lib): $(RESX_RES)

$(RESX_RES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
