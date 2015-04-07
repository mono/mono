thisdir = class/System.ServiceModel.Internals
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Internals.dll
LIB_REFS = System System.Core System.Xml
ifneq (2.1, $(FRAMEWORK_VERSION))
	LIB_REFS += System.Configuration
endif
LIB_MCS_FLAGS = /unsafe

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
