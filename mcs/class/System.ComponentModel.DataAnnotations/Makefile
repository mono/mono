thisdir = class/System.ComponentModel.DataAnnotations
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.ComponentModel.DataAnnotations.dll
LIB_MCS_FLAGS = \
                /r:System.dll \
                /r:System.Core.dll \
                /r:System.Data.dll \
                /r:System.Xml.dll \


ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

# This is a .NET 3.5+ assembly
VALID_PROFILE := $(filter net_2_0 net_4_0 net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ComponentModel.DataAnnotations.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
