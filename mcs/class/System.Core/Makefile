thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll

ifeq ($(PROFILE),build)
CSC_RUNTIME_FLAGS=--profile=aot:output=$(topdir)/class/lib/$(PROFILE)/csc.$(LIBRARY).aotprofile
endif

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,PFX_LEGACY_3_5,FEATURE_NETCORE
LIB_REFS = System
LIB_MCS_FLAGS = $(REFERENCE_SOURCES_FLAGS) -d:INSIDE_SYSCORE -d:LIBC -unsafe -nowarn:436
TXT_RESOURCE_STRINGS = 
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Linq.Expressions/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq.Parallel/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq.Queryable/src/Resources/Strings.resx \
	../../../external/corefx/src/System.IO.Pipes/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Net.Sockets/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Algorithms/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Security.Cryptography.Cng/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Threading/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Collections/src/Resources/Strings.resx \
	../../../external/corefx/src/System.IO.MemoryMappedFiles/src/Resources/Strings.resx

LIBRARY_WARN_AS_ERROR = yes

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5
endif

MOBILE_NATIVE_INTERP := $(filter testing_aot_full_interp, $(PROFILE))

ifdef MOBILE_NATIVE_INTERP
LIB_MCS_FLAGS += -d:FEATURE_COMPILE,FEATURE_COMPILE_TO_METHODBUILDER
else
	ifdef AOT_FRIENDLY_PROFILE
	extra_test_flags := -exclude:NotWorkingLinqInterpreter
	LIB_MCS_FLAGS += -d:NO_FEATURE_STATIC_DELEGATE,FEATURE_MAKE_RUN_METHODS
	else
	LIB_MCS_FLAGS += -d:FEATURE_COMPILE,FEATURE_COMPILE_TO_METHODBUILDER,FEATURE_PDB_GENERATOR
	endif
endif

ifeq (monotouch_runtime, $(PROFILE))
LIB_MCS_FLAGS += -d:FULL_AOT_RUNTIME
endif

CC_PROFILE := $(filter monotouch% xammac, $(PROFILE))
ifdef CC_PROFILE
BUILT_SOURCES = \
	../corlib/CommonCrypto/AesManaged.g.cs \
	../corlib/CommonCrypto/AesCryptoServiceProvider.g.cs

../corlib/CommonCrypto/%.g.cs:
	$(MAKE) -C ../corlib/CommonCrypto
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
XTEST_LIB_REFS = System Facades/System.Threading.Tasks Microsoft.CSharp Facades/System.Text.RegularExpressions Facades/System.Reflection Facades/System.Reflection.Extensions Facades/System.Runtime.InteropServices.RuntimeInformation Facades/System.Reflection.Emit
# FEATURE_INTERPRET, FEATURE_COMPILE, NO_FEATURE_STATIC_DELEGATE
XTEST_LIB_FLAGS = -unsafe -d:FEATURE_COMPILE
XTEST_RESX_RESOURCE_STRING = ../../../external/corefx/src/System.Collections.Immutable/src/Resources/Strings.resx

include ../../build/library.make

TEST_HARNESS_EXCLUDES += $(extra_test_flags)
