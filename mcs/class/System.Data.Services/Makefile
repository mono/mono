thisdir = class/System.Data.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.dll

LIB_REFS = System System.Core System.ServiceModel System.ServiceModel.Web System.Data.Services.Client
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = -d:NET_3_5

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.ServiceModel.Activation
endif

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.ServiceModel System.Core

include ../../build/library.make
