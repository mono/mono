thisdir = nunit20/util
SUBDIRS = 
include ../../build/rules.make

CLEAN_FILES = Transform.resources

LIBRARY = NUnit.Util.dll
LIBRARY_NAME = nunit.util.dll
LIBRARY_SNK = $(topdir)/nunit.key

LOCAL_MCS_FLAGS= \
	/resource:Transform.resources,NUnit.Util.Transform.resources \
	-r:nunit.framework.dll -r:nunit.core.dll -r:System.dll \
	-r:System.Xml.dll /d:MONO /d:StronglyNamedAssembly -debug
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.util.dll.csproj	\
	Transform.resx

%.resources: %.resx
	$(RESGEN) $<

include ../../build/library.make

$(the_lib): Transform.resources
