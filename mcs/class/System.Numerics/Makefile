thisdir = class/System.Numerics
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Numerics.dll
LIB_MCS_FLAGS = -r:System.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))

ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Numerics.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
