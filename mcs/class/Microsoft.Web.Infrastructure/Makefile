thisdir = class/Microsoft.Web.Infrastructure
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Web.Infrastructure.dll
LIB_MCS_FLAGS = -r:System.dll \
		-r:System.Configuration.dll \
		-r:System.Web.dll

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-Microsoft.Web.Infrastructure.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
