thisdir = class/System.Data.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.dll

LIB_REFS = System System.Core System.ServiceModel System.ServiceModel.Web System.Data.Services.Client
LIB_MCS_FLAGS = \
	-d:NET_3_5	\
	-r:System.dll \
	-r:System.Core.dll \
	-r:System.ServiceModel.dll \
	-r:System.ServiceModel.Web.dll \
	-r:System.Data.Services.Client.dll

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.ServiceModel.Activation
LIB_MCS_FLAGS += -r:System.ServiceModel.Activation.dll
endif

TEST_MCS_FLAGS = -r:System.ServiceModel.dll -r:System.Core.dll

include ../../build/library.make
