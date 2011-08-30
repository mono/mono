thisdir = class/Commons.Xml.Relaxng
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = resources/relaxng.rng

OTHER_RES = $(RESOURCE_FILES)

LIBRARY = Commons.Xml.Relaxng.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
	$(foreach r, $(OTHER_RES), /resource:$(r),$(notdir $(r)))
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
EXTRA_DISTFILES = \
	Commons.Xml.Relaxng.Rnc/RncParser.jay	\
	Test/ChangeLog		\
	Test/XmlFiles/ChangeLog	\
	$(wildcard Test/XmlFiles/*.rng) \
	$(wildcard Test/XmlFiles/*.rnc) \
	$(wildcard Test/XmlFiles/*.nvdl) \
	$(wildcard Test/XmlFiles/*.xml) \
	$(wildcard Test/standalone_tests/*.cs) \
	$(RESOURCE_FILES)

Commons.Xml.Relaxng.Rnc/RncParser.cs: Commons.Xml.Relaxng.Rnc/RncParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $(CURDIR)/Commons.Xml.Relaxng.Rnc/RncParser.jay > Commons.Xml.Relaxng.Rnc/RncParser.cs

BUILT_SOURCES = Commons.Xml.Relaxng.Rnc/RncParser.cs

include ../../build/library.make

$(the_lib): $(OTHER_RES)
