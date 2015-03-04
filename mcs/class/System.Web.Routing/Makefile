thisdir = class/System.Web.Routing
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Routing.dll
LIB_REFS = System System.Core System.Web System.Web.Abstractions
LIB_MCS_FLAGS = 

ifdef DEBUG
LIB_MCS_FLAGS += -define:DEBUG
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

# This is a .NET 3.5+ assembly
VALID_PROFILE := $(filter net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Web.Routing.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
