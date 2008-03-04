thisdir = class/System.Dat.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Linq.dll
LIB_MCS_FLAGS = \
                /r:System.dll \
                /r:System.Core.dll \
                /r:System.Data.dll \
                /r:System.Transactions.dll \
                /r:System.Xml.dll \

NO_TEST = yes

# This is a .NET 3.5 only assembly
VALID_PROFILE := $(filter net_3_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Data.Linq.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
