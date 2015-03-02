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

TEST_RESOURCES = \
	Test/System/test-uri-props.txt \
	Test/System/test-uri-props-manual.txt \
	Test/System/test-uri-relative-props.txt

TEST_MCS_FLAGS = -r:System.Drawing.dll -r:Mono.Security.dll -r:System.Data -r:System.Xml.dll -r:System.Core.dll -nowarn:618,672,219,67,169,612 \
	$(foreach f, $(TEST_RESOURCES), -resource:$(f),$(notdir $(f)))

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,SYSTEM_NAMESPACE,MONO,PLATFORM_UNIX
LIB_MCS_FLAGS = -nowarn:618 -d:CONFIGURATION_2_0 $(REFERENCE_SOURCES_FLAGS) -unsafe $(RESOURCE_FILES:%=-resource:%)
TEST_MCS_FLAGS += -r:System.Configuration
PROFILE_ANY_MOBILE := $(filter monotouch monotouch_runtime monodroid xammac mobile mobile_static, $(PROFILE))

RESOURCE_STRINGS = ../../../external/referencesource/System/System.txt

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:INSIDE_SYSTEM
endif
ifeq (monotouch, $(subst _runtime,,$(PROFILE)))
LIB_MCS_FLAGS += -d:SECURITY_DEP
endif
ifeq (monodroid, $(PROFILE))
LIB_MCS_FLAGS += -d:SECURITY_DEP
endif
ifndef PROFILE_ANY_MOBILE
FINAL_MCS_FLAGS = -r:System.Configuration.dll -d:CONFIGURATION_DEP
endif

#
# Flags used to build the secxml version of System.
#
ifeq (secxml/, $(intermediate))
LOCAL_MCS_FLAGS = -lib:$(bare_libdir) 
LIB_REFS += System.Xml MonoSecurity=Mono.Security
LIB_MCS_FLAGS += -d:SECURITY_DEP -d:XML_DEP -r:PrebuiltSystem=$(bare_libdir)/System.dll
endif

#
# Flags used to build the final version of System (when intermediate is not defined)
#
ifndef intermediate
LIB_REFS += System.Xml MonoSecurity=Mono.Security
LIB_MCS_FLAGS += -d:SECURITY_DEP -d:XML_DEP -r:PrebuiltSystem=$(secxml_libdir)/System.dll $(FINAL_MCS_FLAGS)
endif

EXTRA_DISTFILES = \
	Test/test-config-file				\
	Test/test-config-file-net-2.0			\
	Test/System.Security.Cryptography.X509Certificates/pkits/ChangeLog	\
	Test/System.Security.Cryptography.X509Certificates/pkits/Makefile	\
	Test/System.Security.Cryptography.X509Certificates/pkits/README		\
	Test/System.Security.Cryptography.X509Certificates/pkits/x509build.cs	\
	$(RESOURCE_FILES)				\
	$(TEST_RESOURCES)

include ../../build/library.make

system_library_deps := \
	$(secxml_libdir)/System.dll 		\
	$(the_libdir_base)System.Xml.dll	\
	$(the_libdir_base)Mono.Security.dll	\
	$(bare_libdir)/System.dll

ifndef PROFILE_ANY_MOBILE
system_library_deps += $(the_libdir_base)System.Configuration.dll	
endif

artifacts = $(system_library_deps) 			\
	$(bare_libdir)/System.Xml.dll			\
	$(the_libdir_base)Mono.Security.dll		\
	$(the_libdir_base)System.Configuration.dll

.NOTPARALLEL: $(system_library_deps)

$(the_libdir_base)System.dll: $(system_library_deps) 

ifeq (bare/,$(intermediate))
build-bare:
else
$(bare_libdir)/System.dll:
	$(MAKE) intermediate=bare/ $(bare_libdir)/System.dll
endif

ifneq (secxml/,$(intermediate))
$(secxml_libdir)/System.dll: $(bare_libdir)/System.dll $(bare_libdir)/System.Xml.dll $(the_libdir_base)Mono.Security.dll $(bare_libdir)/System.dll
	$(MAKE) intermediate=secxml/ $(secxml_libdir)/System.dll
else
build-sec:
endif

$(the_libdir_base)System.Xml.dll:
	(cd ../System.XML; $(MAKE) $@)

$(bare_libdir)/System.Xml.dll:
	(cd ../System.XML; $(MAKE) $@)

$(the_libdir_base)Mono.Security.dll:
	(cd ../Mono.Security; $(MAKE))

$(the_libdir_base)System.Configuration.dll:
	(cd ../System.Configuration; $(MAKE))

$(build_lib): $(CYCLIC_DEP_FILES)

$(test_lib): $(test_lib).config $(TEST_RESOURCES)

$(test_lib).config: Test/test-config-file
	cp $< $@

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
