thisdir = class/Commons.Xml.Relaxng
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = resources/relaxng.rng

OTHER_RES = $(RESOURCE_FILES)

LIBRARY = Commons.Xml.Relaxng.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
	$(foreach r, $(OTHER_RES), /resource:$(r),$(notdir $(r)))
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
EXTRA_DISTFILES = \
	Commons.Xml.Relaxng.Rnc/RncParser.jay	\
	Test/ChangeLog		\
	Test/XmlFiles/ChangeLog	\
	Test/XmlFiles/SimpleElementPattern1.xml	\
	Test/XmlFiles/SimpleElementPattern1.rng	\
	Test/XmlFiles/SimpleElementPattern2.xml	\
	Test/XmlFiles/SimpleElementPattern2.rng	\
	Test/XmlFiles/team.xml	\
	Test/XmlFiles/team.rng	\
	Test/XmlFiles/relaxng.rng	\
	Test/XmlFiles/relaxng.rnc	\
	$(wildcard Test/standalone_tests/*.cs) \
	$(RESOURCE_FILES)

Commons.Xml.Relaxng.Rnc/RncParser.cs: Commons.Xml.Relaxng.Rnc/RncParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs Commons.Xml.Relaxng.Rnc/RncParser.jay > Commons.Xml.Relaxng.Rnc/RncParser.cs

CLEAN_FILES = Commons.Xml.Relaxng.Rnc/RncParser.cs

include ../../build/library.make
