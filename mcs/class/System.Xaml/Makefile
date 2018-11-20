thisdir = class/System.Xaml
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = 

LIBRARY = System.Xaml.dll
LIB_REFS = System System.Xml System.Core
LIB_MCS_FLAGS = $(RESOURCE_FILES:%=/resource:%)

TEST_RESOURCE_FILES = \
	$(wildcard Test/XmlFiles/*.xml) \
	$(wildcard Test/XmlFiles/*.xaml)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_RESOURCE_FILES)

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Xaml.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
