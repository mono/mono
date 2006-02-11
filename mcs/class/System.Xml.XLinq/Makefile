thisdir = class/System.Xml.XLinq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.XLinq.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Query.dll \
	        /r:System.Xml.dll \
		/d:LIST_BASED

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

# This is a .NET 2.0 only assembly
VALID_PROFILE := $(filter net_2_0_bootstrap net_2_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Xml.XLinq.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
