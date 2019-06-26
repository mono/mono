thisdir = class/Commons.Xml.Relaxng
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = resources/relaxng.rng

OTHER_RES = $(RESOURCE_FILES)

LIBRARY = Commons.Xml.Relaxng.dll
LIB_REFS = System System.Xml
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = \
	$(foreach r, $(OTHER_RES), /resource:$(r),$(notdir $(r)))

TEST_RESOURCE_FILES = \
	$(wildcard Test/XmlFiles/*.rng) \
	$(wildcard Test/XmlFiles/*.rnc) \
	$(wildcard Test/XmlFiles/*.nvdl) \
	$(wildcard Test/XmlFiles/*.xml)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = \
	Commons.Xml.Relaxng.Rnc/RncParser.jay	\
	$(wildcard Test/standalone_tests/*.cs) \
	$(RESOURCE_FILES) \
	$(TEST_RESOURCE_FILES)

Commons.Xml.Relaxng.Rnc/RncParser.cs: Commons.Xml.Relaxng.Rnc/RncParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv -o Commons.Xml.Relaxng.Rnc/RncParser.cs $< < $(topdir)/jay/skeleton.cs

BUILT_SOURCES = Commons.Xml.Relaxng.Rnc/RncParser.cs

include ../../build/library.make

$(the_lib): $(OTHER_RES)
