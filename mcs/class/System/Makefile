thisdir = class/System
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/lib/$(PROFILE)

LIBRARY = System.dll

ifneq (2.1, $(FRAMEWORK_VERSION))
RESOURCE_FILES = \
	resources/Asterisk.wav	\
	resources/Beep.wav	\
	resources/Exclamation.wav	\
	resources/Hand.wav	\
	resources/Question.wav
endif

TEST_RESOURCE_FILES = \
	Test/System/test-uri-props.txt \
	Test/System/test-uri-props-manual.txt \
	Test/System/test-uri-relative-props.txt \
	Test/compressed.bin

XTEST_RESOURCE_FILES = \
	Test/TestData/testclienteku.contoso.com.pfx \
	Test/TestData/testnoeku.contoso.com.pfx \
	Test/TestData/testselfsignedservereku.contoso.com.pfx \
	Test/TestData/testservereku.contoso.com.pfx

USE_XTEST_REMOTE_EXECUTOR = YES
XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks Facades/System.Runtime.InteropServices.RuntimeInformation System.Net.Http
LIB_MCS_FLAGS = -d:COREFX -d:CONFIGURATION_2_0 -d:SYSTEM_NET_PRIMITIVES_DLL -d:XML_DEP -d:SECURITY_DEP $(REFERENCE_SOURCES_FLAGS) -unsafe $(RESOURCE_FILES:%=-resource:%) -nowarn:436
XTEST_MCS_FLAGS += $(XTEST_RESOURCE_FILES:%=-resource:%)

ifndef NO_MONO_SECURITY
MONO_SECURITY := Mono.Security
LIB_MCS_FLAGS += -d:MONO_SECURITY_ALIAS
LIB_REFS += MonoSecurity=Mono.Security
else
LIB_MCS_FLAGS += -d:INSIDE_SYSTEM
endif

ifndef MOBILE_PROFILE
LIB_MCS_FLAGS += -d:CODEDOM
TEST_LIB_REFS = $(MONO_SECURITY) System.Data System.Xml System.Core System.Configuration

ifndef NO_SYSTEM_DRAWING_DEPENDENCY
TEST_LIB_REFS += System.Drawing
endif

else
LIB_MCS_FLAGS += -nowarn:618
TEST_LIB_REFS = $(MONO_SECURITY) System.Data System.Xml System.Core
endif

TEST_MCS_FLAGS = -nowarn:618,672,219,67,169,612 \
	$(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,SYSTEM_NAMESPACE,MONO,PLATFORM_UNIX
ifndef NO_PROCESS_START
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_PROCESS_START
TEST_MCS_FLAGS += -d:MONO_FEATURE_PROCESS_START
endif

ifdef MONO_FEATURE_APPLETLS
LIB_MCS_FLAGS += -d:MONO_FEATURE_APPLETLS
endif

ifndef AOT_FRIENDLY_PROFILE
LIB_MCS_FLAGS += -d:FEATURE_COMPILED
endif

ifdef ONLY_APPLETLS
LIB_MCS_FLAGS += -d:ONLY_APPLETLS
endif

ifdef MONO_FEATURE_APPLE_X509
LIB_MCS_FLAGS += -d:MONO_FEATURE_APPLE_X509
endif

ifndef PROFILE_DISABLE_BTLS
ifdef HAVE_BTLS
LIB_MCS_FLAGS += -d:MONO_FEATURE_BTLS
endif
endif

ifdef ENABLE_GSS
ifndef NO_GSS
LIB_MCS_FLAGS += -d:ENABLE_GSS
endif
endif

ifndef NO_THREAD_ABORT
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

ifndef NO_THREAD_SUSPEND_RESUME
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_SUSPEND_RESUME
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_SUSPEND_RESUME
endif

ifndef NO_MULTIPLE_APPDOMAINS
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_MULTIPLE_APPDOMAINS
TEST_MCS_FLAGS += -d:MONO_FEATURE_MULTIPLE_APPDOMAINS
endif

TXT_RESOURCE_STRINGS = ../referencesource/System/System.txt


API_BIN_REFS := System.Net.Http System.Xml System.Core System.Numerics

ifndef MOBILE_PROFILE
API_BIN_REFS += System.Configuration
LIB_MCS_FLAGS += -d:CONFIGURATION_DEP
endif

EXTRA_DISTFILES = \
	Test/test-config-file				\
	Test/System.Security.Cryptography.X509Certificates/pkits/Makefile	\
	Test/System.Security.Cryptography.X509Certificates/pkits/README		\
	Test/System.Security.Cryptography.X509Certificates/pkits/x509build.cs	\
	$(RESOURCE_FILES)				\
	$(TEST_RESOURCE_FILES)			\
	$(XTEST_RESOURCE_FILES)

include ../../build/library.make

$(test_lib_output): $(TEST_RESOURCE_FILES) $(test_lib_dir)

$(xtest_lib_output): $(XTEST_RESOURCE_FILES)

# Helper target to run the perl regex test suite
regex-check:
	$(MAKE) check FIXTURE=System.Text.RegularExpressions.PerlTest
