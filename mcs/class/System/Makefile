thisdir = class/System
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/lib/$(PROFILE)

ASSEMBLY = System.dll

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Collections.Concurrent/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Collections/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Buffers/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Private.Uri/src/Resources/Strings.resx \
	../../../external/corefx/src/System.IO.Ports/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Net.HttpListener/src/Resources/Strings.resx

TEST_RESOURCES = \
	Test/System/test-uri-props.txt \
	Test/System/test-uri-props-manual.txt \
	Test/System/test-uri-relative-props.txt

ifndef NO_MONO_SECURITY
MONO_SECURITY=Mono.Security
MONO_SECURITY_DLL=$(the_assemblydir_base)Mono.Security.dll
endif

ifndef MOBILE_PROFILE
TEST_LIB_REFS = $(MONO_SECURITY) System.Data System.Xml System.Core System.Configuration

ifndef NO_SYSTEM_DRAWING_DEPENDENCY
TEST_LIB_REFS += System.Drawing
endif

else
TEST_LIB_REFS = $(MONO_SECURITY) System.Data System.Xml System.Core
endif

TEST_MCS_FLAGS = -nowarn:618,672,219,67,169,612 \
	$(foreach f, $(TEST_RESOURCES), -resource:$(f),$(notdir $(f)))

TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file

ifndef NO_PROCESS_START
TEST_MCS_FLAGS += -d:MONO_FEATURE_PROCESS_START
endif

ifndef NO_THREAD_ABORT
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

ifndef NO_THREAD_SUSPEND_RESUME
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_SUSPEND_RESUME
endif

ifndef NO_MULTIPLE_APPDOMAINS
TEST_MCS_FLAGS += -d:MONO_FEATURE_MULTIPLE_APPDOMAINS
endif

TXT_RESOURCE_STRINGS = ../referencesource/System/System.txt

EXTRA_DISTFILES = \
	common.sources					\
	common_networking.sources		\
	Test/test-config-file				\
	Test/System.Security.Cryptography.X509Certificates/pkits/Makefile	\
	Test/System.Security.Cryptography.X509Certificates/pkits/README		\
	Test/System.Security.Cryptography.X509Certificates/pkits/x509build.cs	\
	$(RESOURCE_FILES)				\
	$(TEST_RESOURCES)

include ../../build/msbuild.make

$(test_lib): $(TEST_RESOURCES)

CLEAN_FILES = $(test_lib).config $(the_assemblydir_base)bare/System.dll $(the_assemblydir_base)secxml/System.dll $(the_assemblydir_base)bare/System.dll.mdb $(the_assemblydir_base)secxml/System.dll.mdb

# Helper target to run the perl regex test suite
regex-check:
	$(MAKE) check FIXTURE=System.Text.RegularExpressions.PerlTest
