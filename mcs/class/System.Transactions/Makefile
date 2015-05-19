thisdir = class/System.Transactions
SUBDIRS = 
include ../../build/rules.make

MOBILE_PROFILE := $(filter monotouch monodroid xammac mobile mobile_static, $(PROFILE))

LIBRARY = System.Transactions.dll
ifdef MOBILE_PROFILE
LIB_REFS = System
LIB_MCS_FLAGS = /r:$(corlib) /define:MOBILE
else
LIB_REFS = System System.Configuration
LIB_MCS_FLAGS = /r:$(corlib)
endif

TEST_MCS_FLAGS = /nowarn:1595 $(LIB_MCS_FLAGS)

include ../../build/library.make
