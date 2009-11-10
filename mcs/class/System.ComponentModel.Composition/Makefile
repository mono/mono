thisdir = class/System.ComponentModel.Composition
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ComponentModel.Composition.dll
LIB_MCS_FLAGS = -r:System.dll -r:System.Core.dll -d:CLR40 -resource:$(RESOURCE_FILES)

RESOURCE_FILES = src/ComponentModel/Strings.resx

EXTRA_DISTFILES = \
	$(RESOURCE_FILES)

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ComponentModel.Composition.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
