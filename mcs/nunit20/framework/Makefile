thisdir = nunit20/framework
SUBDIRS = 
include ../../build/rules.make

LIBRARY = NUnit.Framework.dll
LIB_MCS_FLAGS = \
	/resource:Transform.resources,NUnit.Framework.Transform.resources \
	/r:System.Xml.dll /r:System.dll
NO_TEST = yo

EXTRA_DISTFILES = \
	Transform.resources 		\
	Transform.resx			\
	nunit.framework.dll.csproj	\
	Results.xsd			\
	Results.xsx			\
	Summary.xslt

include ../../build/library.make
