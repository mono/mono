thisdir = nunit20/util
SUBDIRS = 
include ../../build/rules.make

LIBRARY = NUnit.Util.dll
LIBRARY_NAME = nunit.util.dll
LOCAL_MCS_FLAGS= \
	/resource:Transform.resources,NUnit.Util.Transform.resources \
	-r:nunit.framework.dll -r:nunit.core.dll -r:System.dll \
	-r:System.Xml.dll /d:MONO /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = \
	nunit.util.dll.csproj	\
	Transform.resx

%.resources: %.resx
	MONO_PATH=$(topdir)/class/lib/$(PROFILE):$$MONO_PATH $(INTERNAL_RESGEN) $<

include ../../build/library.make

$(the_lib): Transform.resources
