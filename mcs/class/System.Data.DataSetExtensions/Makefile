thisdir = class/System.Data.DataSetExtensions
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Data.DataSetExtensions.dll

RESOURCE_STRINGS = ../../../external/referencesource/System.Data.DataSetExtensions/System.Data.DataSetExtensions.txt

LIB_REFS = System.Core System System.Data System.Xml

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml)  -nowarn:219 -nowarn:169 $(NUNIT_RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES) \
	Test/System.Data/testdataset1.xml

include ../../build/library.make

