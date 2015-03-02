thisdir = class/System.Web.Abstractions
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Abstractions.dll
LIB_REFS = System System.Core System.Web
LIB_MCS_FLAGS =

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

# This is a .NET 3.5+ assembly
VALID_PROFILE := $(filter net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Web.Abstractions.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
