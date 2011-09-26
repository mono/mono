thisdir = class/System.Web.ApplicationServices
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.ApplicationServices.dll
LIB_MCS_FLAGS = -r:System.dll \
		-r:System.Configuration.dll \
		-d:SYSTEM_WEB_APPLICATIONSERVICES

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Web.ApplicationServices.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
