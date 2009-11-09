thisdir = class/System.ComponentModel.Composition
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ComponentModel.Composition.dll
LIB_MCS_FLAGS = -r:System.dll -r:System.Core.dll -resource:./src/ComponentModel/Strings.resx

RESX_RESOURCES = ./src/ComponentModel/Strings.resx

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ComponentModel.Composition.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
