thisdir = class/System.Configuration
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Configuration.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Xml.dll -r:System.Security.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

# This is a .NET 2.0+ only assembly
VALID_PROFILE := $(filter net_2_0 net_4_0_bootstrap net_4_0 net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Configuration.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
