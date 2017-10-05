thisdir = class/Microsoft.Web.Infrastructure
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Web.Infrastructure.dll
LIB_REFS = System System.Configuration System.Web
KEYFILE = ../winfx.pub
LIB_MCS_FLAGS =

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-Microsoft.Web.Infrastructure.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
