thisdir = class/System.Transactions
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Transactions.dll
KEYFILE = ../ecma.pub
ifdef MOBILE_PROFILE
LIB_REFS = System
LIB_MCS_FLAGS = /define:MOBILE
else
LIB_REFS = System System.Configuration
LIB_MCS_FLAGS =
endif

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Transactions.Local/src/Resources/Strings.resx

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
