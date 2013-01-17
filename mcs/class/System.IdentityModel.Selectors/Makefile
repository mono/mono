thisdir = class/System.IdentityModel.Selectors
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.IdentityModel.Selectors.dll
LIB_MCS_FLAGS = \
		/d:NET_3_0	\
		/r:System.dll \
		/r:System.Xml.dll \
		/r:System.Security.dll \
		/r:System.Runtime.Serialization.dll \
		/r:System.IdentityModel.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter net_2_0 net_4_0 net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.IdentityModel.Selectors.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
