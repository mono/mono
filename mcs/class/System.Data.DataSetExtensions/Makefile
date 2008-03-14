thisdir = class/System.Data.DataSetExtensions
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Data.DataSetExtensions.dll

LIB_MCS_FLAGS = \
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Data.dll		\
	-r:System.Xml.dll		\
	-r:System.Web.dll		\
	$(OTHER_LIB_MCS_FLAGS) 

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml)  -nowarn:219 -nowarn:169 $(NUNIT_RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES) 

include ../../build/library.make

