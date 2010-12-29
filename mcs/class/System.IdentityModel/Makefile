thisdir = class/System.IdentityModel
SUBDIRS = 
include ../../build/rules.make

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
OTHER_LIB_MCS_FLAGS = -r:System.Web.ApplicationServices.dll
endif

LIBRARY = System.IdentityModel.dll
LIB_MCS_FLAGS = \
		/d:NET_3_0	\
	        /r:System.dll \
	        /r:System.Xml.dll \
	        /r:System.Security.dll \
		/r:System.Web.dll \
		/r:System.Configuration.dll \
		/r:Mono.Security.dll \
	        /r:System.Runtime.Serialization.dll \
	        $(OTHER_LIB_MCS_FLAGS)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	Test/Resources/test.cer \
	Test/Resources/test.pfx \
	Test/Resources/test2.pfx

VALID_PROFILE := $(filter net_2_0 net_4_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.IdentityModel.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
