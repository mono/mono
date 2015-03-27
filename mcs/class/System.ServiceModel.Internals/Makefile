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

EXTRA_DISTFILES = 

VALID_PROFILE := $(filter net_4_5 monotouch monodroid xammac, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.Internals.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
