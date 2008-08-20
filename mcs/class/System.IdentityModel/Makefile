thisdir = class/System.IdentityModel
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.IdentityModel.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Xml.dll \
	        /r:System.Security.dll \
		/r:System.Web.dll \
		/r:Mono.Security.dll \
	        /r:../lib/net_3_0/System.Runtime.Serialization.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	Test/Resources/test.cer \
	Test/Resources/test.pfx \
	Test/Resources/test2.pfx

# This is a WinFX only assembly
VALID_PROFILE := $(filter net_3_0 net_3_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.IdentityModel.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
