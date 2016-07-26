thisdir = class/System.IdentityModel
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.IdentityModel.dll
LIB_REFS = System System.Xml System.Security Mono.Security System.Runtime.Serialization
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

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	Test/Resources/test.cer \
	Test/Resources/test_neg_serial.cer \
	Test/Resources/test.pfx \
	Test/Resources/test2.pfx

include ../../build/library.make
