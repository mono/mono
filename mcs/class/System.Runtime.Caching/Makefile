thisdir = class/System.Runtime.Caching
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.Caching.dll
LIB_MCS_FLAGS = -r:System.dll \
	        -r:System.Data.dll \
		-r:System.Configuration.dll \
		-r:System.Core.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Runtime.Caching.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
