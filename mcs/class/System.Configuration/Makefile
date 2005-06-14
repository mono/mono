thisdir = class/System.Configuration
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Configuration.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = Test/ChangeLog

# This is a .NET 2.0 only assembly
VALID_PROFILE := $(filter net_2_0_bootstrap net_2_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Configuration.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
