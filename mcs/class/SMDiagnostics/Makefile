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

VALID_PROFILE := $(filter net_4_5 monotouch monodroid, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-SMDiagnostics.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
