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

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
