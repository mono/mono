thisdir = class/System.IdentityModel.Selectors
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.IdentityModel.Selectors.dll
LIB_REFS = System System.Xml System.Security System.Runtime.Serialization System.IdentityModel
LIB_MCS_FLAGS = /d:NET_3_0

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.IdentityModel.Selectors.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
