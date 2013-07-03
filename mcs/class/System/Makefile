thisdir = class/System
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/lib/$(PROFILE)

LIBRARY = System.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

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

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS = -nowarn:618 -d:CONFIGURATION_2_0 -unsafe $(RESOURCE_FILES:%=-resource:%)
TEST_MCS_FLAGS += -r:System.Configuration
PROFILE_ANY_MOBILE := $(filter monotouch monodroid, $(PROFILE))
NOT_SL := $(filter net_2_0 net_4_0 net_4_5 monotouch_runtime monodroid mobile, $(PROFILE))
endif
ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:INSIDE_SYSTEM
ifeq (moonlight_raw, $(PROFILE))
LIB_MCS_FLAGS += -d:SECURITY_DEP
endif
endif
ifeq (monotouch, $(PROFILE))
LIB_MCS_FLAGS += -d:FULL_AOT_RUNTIME -d:SECURITY_DEP
endif

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file				\
	Test/test-config-file-net-2.0			\
	Test/System.Security.Cryptography.X509Certificates/pkits/ChangeLog	\
	Test/System.Security.Cryptography.X509Certificates/pkits/Makefile	\
	Test/System.Security.Cryptography.X509Certificates/pkits/README		\
	Test/System.Security.Cryptography.X509Certificates/pkits/x509build.cs	\
	$(RESOURCE_FILES)				\
	$(TEST_RESOURCES)

lib_file := $(wildcard ../lib/$(PROFILE)/System.dll)
ifdef lib_file
ifdef NOT_SL
LIB_MCS_FLAGS += -r:PrebuiltSystem=$(lib_file)
endif
else
USE_BOOT_COMPILE = yes
endif

ifdef USE_BOOT_COMPILE
ifndef PROFILE_MCS_HAS_BOOTSTRAP_FALLBACK
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif
endif

# Because System.dll, Mono.Security.dll System.Xml.dll have a cyclic dependency, we need a two-pass build.
# 1st pass - build System.dll without System.Xml.dll or Mono.Security.dll or System.Configuration.dll references
# 2nd pass - build System.dll with System.Xml.dll and Mono.Security.dll or System.Configuration.dll references

CYCLIC_DEPS :=
CYCLIC_DEP_FILES :=

# define cyclic-dep
# # Note the '$$' -- those are to be delayed till the eval
# CYCLIC_DEPS += $1
# ifndef USE_BOOT_COMPILE
# FILE_$2 := $$(wildcard ../lib/$(PROFILE)/$1)
# endif
# ifdef FILE_$2
# CYCLIC_DEP_FILES += $$(FILE_$2)
# LIB_MCS_FLAGS += -d:$2 -r:$1
# endif
# endef

ifneq (2.1, $(FRAMEWORK_VERSION))
# $(eval $(call cyclic-dep,System.Xml.dll,XML_DEP))
CYCLIC_DEPS += System.Xml.dll
ifndef USE_BOOT_COMPILE
FILE_XML_DEP := $(wildcard ../lib/$(PROFILE)/System.Xml.dll)
endif
ifdef FILE_XML_DEP
CYCLIC_DEP_FILES += $(FILE_XML_DEP)
LIB_MCS_FLAGS += -d:XML_DEP -r:System.Xml.dll
endif

endif

ifdef NOT_SL
# $(eval $(call cyclic-dep,Mono.Security.dll,SECURITY_DEP))
CYCLIC_DEPS += Mono.Security.dll
ifndef USE_BOOT_COMPILE
FILE_SECURITY_DEP := $(wildcard ../lib/$(PROFILE)/Mono.Security.dll)
endif
ifdef FILE_SECURITY_DEP
CYCLIC_DEP_FILES += $(FILE_SECURITY_DEP)
LIB_MCS_FLAGS += -d:SECURITY_DEP -r:MonoSecurity=Mono.Security.dll
endif

ifndef PROFILE_ANY_MOBILE
# $(eval $(call cyclic-dep,System.Configuration.dll,CONFIGURATION_DEP))
CYCLIC_DEPS += System.Configuration.dll
ifndef USE_BOOT_COMPILE
FILE_CONFIGURATION_DEP := $(wildcard ../lib/$(PROFILE)/System.Configuration.dll)
endif
ifdef FILE_CONFIGURATION_DEP
CYCLIC_DEP_FILES += $(FILE_CONFIGURATION_DEP)
LIB_MCS_FLAGS += -d:CONFIGURATION_DEP -r:System.Configuration.dll
endif
endif

endif

MISSING_DEPS := $(filter-out $(notdir $(CYCLIC_DEP_FILES)), $(CYCLIC_DEPS))
ifdef MISSING_DEPS
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
all-local: handle-missing-deps
.PHONY: handle-missing-deps
handle-missing-deps:
	@echo "** Warning: System.dll built without parts that depend on: $(MISSING_DEPS)"
	@for f in $(MISSING_DEPS:%=../lib/$(PROFILE)/%); do \
	  test ! -f $$f || { echo "   purging stale $$f"; rm -f $$f; }; done
endif

include ../../build/library.make

$(build_lib): $(CYCLIC_DEP_FILES)

$(test_lib): $(test_lib).config $(TEST_RESOURCES)

ifeq (net_2_0, $(PROFILE))
$(test_lib).config: Test/test-config-file-net-2.0
	cp $< $@
else
$(test_lib).config: Test/test-config-file
	cp $< $@
endif

CLEAN_FILES = $(test_lib).config

# run the PKITS tests only if the data was installed/activated, otherwise ignore them
ifeq (net_2_0, $(PROFILE))
pkits_files := $(wildcard Test/System.Security.Cryptography.X509Certificates/pkits/hint)
ifndef pkits_files
TEST_HARNESS_EXCLUDES = -exclude=$(PLATFORM_TEST_HARNESS_EXCLUDES)NotWorking,ValueAdd,CAS,InetAccess,PKITS
TEST_HARNESS_EXCLUDES_ONDOTNET = -exclude=$(PLATFORM_TEST_HARNESS_EXCLUDES)NotDotNet,CAS,PKITS
endif
endif

# Helper target to run the perl regex test suite
regex-check:
	$(MAKE) check FIXTURE=System.Text.RegularExpressions.PerlTest
