thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

ASSEMBLY = System.Core.dll

ifeq ($(PROFILE),build)
CSC_RUNTIME_FLAGS=--profile=aot:output=$(topdir)/class/lib/$(PROFILE)/csc.$(ASSEMBLY).aotprofile
endif

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Linq.Expressions/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq.Parallel/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq.Queryable/src/Resources/Strings.resx

ifdef AOT_FRIENDLY_PROFILE
extra_test_flags := -exclude:NotWorkingInterpreter
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/msbuild.make

TEST_HARNESS_EXCLUDES += $(extra_test_flags)
