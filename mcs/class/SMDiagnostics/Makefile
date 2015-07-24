thisdir = class/SMDiagnostics
SUBDIRS = 
include ../../build/rules.make

LIBRARY = SMDiagnostics.dll
LIB_REFS = System System.Core System.Xml System.ServiceModel.Internals
LIB_MCS_FLAGS =
ifneq (2.1, $(FRAMEWORK_VERSION))
	LIB_REFS += System.Configuration
else
	LIB_MCS_FLAGS += -d:NO_CONFIGURATION
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = 

include ../../build/library.make
