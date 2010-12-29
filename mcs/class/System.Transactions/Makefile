thisdir = class/System.Transactions
SUBDIRS = 
include ../../build/rules.make

MOBILE_PROFILE := $(filter monotouch monodroid, $(PROFILE))

LIBRARY = System.Transactions.dll
ifdef MOBILE_PROFILE
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /define:MOBILE
else
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Configuration.dll
endif

TEST_MCS_FLAGS = /nowarn:1595 $(LIB_MCS_FLAGS)

# This is a .NET 2.0+ only assembly
VALID_PROFILE := $(filter net_2_0 net_4_0 monotouch monodroid, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Transactions.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif


include ../../build/library.make
