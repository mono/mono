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

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Collections.Concurrent/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Collections/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Buffers/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Private.Uri/src/Resources/Strings.resx

TEST_RESOURCES = \
	Test/System/test-uri-props.txt \
	Test/System/test-uri-props-manual.txt \
	Test/System/test-uri-relative-props.txt

LIB_MCS_FLAGS = -d:CONFIGURATION_2_0 $(REFERENCE_SOURCES_FLAGS) -unsafe $(RESOURCE_FILES:%=-resource:%) -nowarn:436

ifndef NO_MONO_SECURITY
MONO_SECURITY=Mono.Security
MONO_SECURITY_REF=MonoSecurity=Mono.Security
MONO_SECURITY_DLL=$(the_libdir_base)Mono.Security.dll
EXTERN_ALIAS_FLAGS = -d:MONO_SECURITY_ALIAS
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
	$(foreach f, $(TEST_RESOURCES), -resource:$(f),$(notdir $(f)))

TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,SYSTEM_NAMESPACE,MONO,PLATFORM_UNIX
ifndef NO_PROCESS_START
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_PROCESS_START
TEST_MCS_FLAGS += -d:MONO_FEATURE_PROCESS_START
endif

ifdef MONO_FEATURE_APPLETLS
LIB_MCS_FLAGS += -d:MONO_FEATURE_APPLETLS
endif

ifdef ONLY_APPLETLS
LIB_MCS_FLAGS += -d:ONLY_APPLETLS
endif

ifndef PROFILE_DISABLE_BTLS
ifdef HAVE_BTLS

LIB_MCS_FLAGS += -d:MONO_FEATURE_BTLS

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

#
# Flags used to build the secxml version of System.
#
ifeq (secxml/, $(intermediate))
LOCAL_MCS_FLAGS =
LIB_REFS += bare/System.Xml $(MONO_SECURITY_REF)
LIB_MCS_FLAGS += -d:SECURITY_DEP -d:XML_DEP $(EXTERN_ALIAS_FLAGS)
endif

#
# Flags used to build the final version of System (when intermediate is not defined)
#
ifndef intermediate
LIB_REFS += System.Xml $(MONO_SECURITY_REF)
LIB_MCS_FLAGS += -d:SECURITY_DEP -d:XML_DEP $(EXTERN_ALIAS_FLAGS)

ifndef MOBILE_PROFILE
LIB_REFS += System.Configuration
LIB_MCS_FLAGS += -d:CONFIGURATION_DEP
endif

endif

EXTRA_DISTFILES = \
	common.sources					\
	Test/test-config-file				\
	Test/System.Security.Cryptography.X509Certificates/pkits/Makefile	\
	Test/System.Security.Cryptography.X509Certificates/pkits/README		\
	Test/System.Security.Cryptography.X509Certificates/pkits/x509build.cs	\
	$(RESOURCE_FILES)				\
	$(TEST_RESOURCES)

include ../../build/library.make

system_library_deps := \
	$(secxml_libdir)/System.dll 		\
	$(the_libdir_base)System.Xml.dll	\
	$(MONO_SECURITY_DLL)				\
	$(bare_libdir)/System.dll

ifndef MOBILE_PROFILE
system_library_deps += $(the_libdir_base)System.Configuration.dll	
endif

artifacts = $(system_library_deps) 			\
	$(bare_libdir)/System.Xml.dll			\
	$(MONO_SECURITY_DLL)				\
	$(the_libdir_base)System.Configuration.dll

.NOTPARALLEL: $(system_library_deps)

$(the_libdir_base)System.dll: $(system_library_deps) ../Mono.Security/Makefile

ifeq (bare/,$(intermediate))
build-bare:
else
$(bare_libdir)/System.dll:
	$(MAKE) intermediate=bare/ $(bare_libdir)/System.dll
endif

ifneq (secxml/,$(intermediate))
$(secxml_libdir)/System.dll: $(bare_libdir)/System.dll $(bare_libdir)/System.Xml.dll $(MONO_SECURITY_DLL) $(bare_libdir)/System.dll
	$(MAKE) intermediate=secxml/ $(secxml_libdir)/System.dll
else
build-sec:
endif

$(the_libdir_base)System.Xml.dll:
	(cd ../System.XML; $(MAKE) $@)

$(bare_libdir)/System.Xml.dll:
	(cd ../System.XML; $(MAKE) $@)

$(the_libdir_base)Mono.Security.dll: ../Mono.Security/Makefile
	(cd ../Mono.Security; $(MAKE))

$(the_libdir_base)System.Configuration.dll:
	(cd ../System.Configuration; $(MAKE))

$(build_lib): $(CYCLIC_DEP_FILES)

$(test_lib): $(TEST_RESOURCES)

CLEAN_FILES = $(test_lib).config $(bare_libdir)/System.dll $(secxml_libdir)/System.dll $(bare_libdir)/System.dll.mdb $(secxml_libdir)/System.dll.mdb

# Helper target to run the perl regex test suite
regex-check:
	$(MAKE) check FIXTURE=System.Text.RegularExpressions.PerlTest

#Debuging target
fresh: clean
	rm -rf ../../class/lib/$(PROFILE)/bare ../../class/lib/$(PROFILE)/bare/ $(system_library_deps)

ifndef intermediate
ifneq ($(PROFILE),basic)
csproj-local:
	$(MAKE) csproj-local intermediate=bare/
	$(MAKE) csproj-local intermediate=secxml/
endif
endif
