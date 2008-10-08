thisdir = class/System.EnterpriseServices
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.EnterpriseServices.dll
LIB_MCS_FLAGS = /nowarn:0168 /nowarn:0162 /r:$(corlib)
NO_TEST = yes

ifneq (net_1_1, $(PROFILE))
LIB_MCS_FLAGS += /r:System.Transactions.dll
endif

EXTRA_DISTFILES = System.EnterpriseServices.Internal/ChangeLog

include ../../build/library.make
