thisdir = class/System.Xml.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.Linq.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.Xml.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

# This is a .NET 3.5 only assembly
VALID_PROFILE := $(filter net_3_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Xml.Linq.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
