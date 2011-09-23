thisdir = class/Mono.Simd
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Simd.dll
LIB_MCS_FLAGS = -r:$(corlib) -r:System.Core.dll /unsafe
TEST_MCS_FLAGS = -r:Mono.Simd.dll

NO_TEST = yes


# This is a .NET 2.0+ only assembly
VALID_PROFILE := $(filter net_2_0 net_4_0 net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-Mono.Simd.dll
NO_INSTALL = yes
NO_TEST = yes
endif

include ../../build/library.make

