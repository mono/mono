thisdir = class/System.Xaml
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = 

LIBRARY = System.Xaml.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Xml.dll \
		/r:System.Core.dll \
	        $(RESOURCE_FILES:%=/resource:%)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_EXTRA_DISTFILES)
TEST_EXTRA_DISTFILES = \
	Test/XmlFiles/*.xml \
	Test/XmlFiles/*.xaml

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Xaml.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
