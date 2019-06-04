thisdir = class/Mono.Runtime.Tests
SUBDIRS = 
include ../../build/rules.make

#
# This directory is used to run the JIT regression tests in mini/ using the
# standard nunit-lite test runner.
#
LIBRARY = Mono.Runtime.Tests.dll
NO_BUILD = 1
NO_INSTALL = 1
LIB_REFS = System System.Core System.Numerics System.Numerics.Vectors
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -unsafe -d:__MOBILE__ -nowarn:CS0169,CS0649,CS0414,CS0618,CS0219,CS0168

ifneq ($(PROFILE),monodroid)
LIB_REFS += Mono.Simd
endif

include ../../build/library.make

