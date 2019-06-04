thisdir = class/System.IdentityModel
SUBDIRS = 
include ../../build/rules.make

ifndef NO_MONO_SECURITY
MONO_SECURITY=Mono.Security
endif

LIBRARY = System.IdentityModel.dll
LIB_REFS = System System.Xml System.Security $(MONO_SECURITY) System.Runtime.Serialization
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = \
		/d:NET_3_0	\
	        $(OTHER_LIB_MCS_FLAGS)

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Configuration
endif

ifndef NO_SYSTEM_WEB_DEPENDENCY
ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Web
endif
endif

ifndef NO_SYSTEM_WEB_APPSERVICES_DEPENDENCY
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.Web.ApplicationServices
endif
endif

TEST_RESOURCE_FILES = \
	Test/Resources/test.cer \
	Test/Resources/test_neg_serial.cer \
	Test/Resources/test.pfx \
	Test/Resources/test2.pfx

TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = System System.Xml System.Security System.Runtime.Serialization

ifdef MOBILE_PROFILE
NO_TEST = yes
endif

EXTRA_DISTFILES = $(TEST_RESOURCE_FILES)

include ../../build/library.make
