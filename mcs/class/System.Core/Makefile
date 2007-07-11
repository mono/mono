thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll
LIB_MCS_FLAGS = -langversion:linq -unsafe -d:LIBC /r:$(corlib)
NO_TEST = yes

# This is a .NET > 2.0 only assembly
VALID_PROFILE := $(filter net_2_0_bootstrap net_2_0 net_2_1, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Core.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
