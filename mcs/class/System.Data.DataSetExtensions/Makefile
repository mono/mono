thisdir = class/System.Data.DataSetExtensions
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Data.DataSetExtensions.dll

LIB_MCS_FLAGS = \
	-r:$(corlib)	 		\
	-r:System.Core.dll 		\
	-r:System.dll 			\
	-r:System.Data.dll		\
	-r:System.Xml.dll		\
	-r:System.Web.dll		\
	$(OTHER_LIB_MCS_FLAGS) 

ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml)  -nowarn:219 -nowarn:169 $(NUNIT_RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES) \
	Test/System.Data/testdataset1.xml

include ../../build/library.make

