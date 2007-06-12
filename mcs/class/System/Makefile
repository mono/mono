thisdir = class/System
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/lib/$(PROFILE)

LIBRARY = System.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

RESOURCE_FILES = \
	resources/Asterisk.wav	\
	resources/Beep.wav	\
	resources/Exclamation.wav	\
	resources/Hand.wav	\
	resources/Question.wav

# Because System.dll, Mono.Security.dll System.Xml.dll have a cyclic dependency, we need a two-pass build.
# 1st pass - build System.dll without System.Xml.dll or Mono.Security.dll or System.Configuration.dll references
# 2nd pass - build System.dll with System.Xml.dll and Mono.Security.dll or System.Configuration.dll references

TEST_RESOURCES = \
	Test/System/test-uri-props.txt \
	Test/System/test-uri-props-manual.txt \
	Test/System/test-uri-relative-props.txt

TEST_MCS_FLAGS = -r:System.Drawing.dll -r:Mono.Security.dll -nowarn:1595 -nowarn:0618 -nowarn:219 -nowarn:67 -nowarn:169 -nowarn:612 \
	$(foreach f, $(TEST_RESOURCES), -resource:$(f),$(notdir $(f)))

XML_DEP := System.Xml.dll
XML_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(XML_DEP))
CYCLIC_DEPS := $(XML_DEP)
CYCLIC_DEP_FILES := $(XML_DEP_FILE)

ifeq (2, $(FRAMEWORK_VERSION_MAJOR))
# Happens on net_2_0_bootstrap and net_2_0 profiles
SECURITY_DEP := Mono.Security.dll
SECURITY_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(SECURITY_DEP))
CONFIGURATION_DEP := System.Configuration.dll
CONFIGURATION_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(CONFIGURATION_DEP))
CYCLIC_DEPS += $(SECURITY_DEP) $(CONFIGURATION_DEP)
CYCLIC_DEP_FILES += $(SECURITY_DEP_FILE) $(CONFIGURATION_DEP_FILE)
LIB_MCS_FLAGS = -nowarn:618 -d:CONFIGURATION_2_0 -unsafe $(RESOURCE_FILES:%=-resource:%)
TEST_MCS_FLAGS += -r:System.Configuration
endif

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file				\
	Test/System.Security.Cryptography.X509Certificates/pkits/ChangeLog	\
	Test/System.Security.Cryptography.X509Certificates/pkits/Makefile	\
	Test/System.Security.Cryptography.X509Certificates/pkits/README		\
	Test/System.Security.Cryptography.X509Certificates/pkits/x509build.cs	\
	$(RESOURCE_FILES)				\
	$(TEST_RESOURCES)

lib_file := $(wildcard ../lib/$(PROFILE)/System.dll)
ifndef lib_file
USE_BOOT_COMPILE = yes
endif

ifeq (net_2_0, $(PROFILE))
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)
endif

ifdef USE_BOOT_COMPILE
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

MISSING_DEPS := $(filter-out $(notdir $(CYCLIC_DEP_FILES)), $(CYCLIC_DEPS))
ifdef MISSING_DEPS
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
all-local: echo-warning
.PHONY: echo-warning
echo-warning:
	@echo "** Warning: System.dll built without parts that depend on: $(MISSING_DEPS)"
endif

include ../../build/library.make

ifdef XML_DEP_FILE
LIB_MCS_FLAGS += -define:XML_DEP -r:$(XML_DEP)
$(the_lib): $(XML_DEP_FILE)
endif

ifdef SECURITY_DEP_FILE
LIB_MCS_FLAGS += -define:SECURITY_DEP -r:Mono.Security.dll
$(the_lib): $(SECURITY_DEP_FILE)
endif

ifdef CONFIGURATION_DEP_FILE
LIB_MCS_FLAGS += -define:CONFIGURATION_DEP -r:$(CONFIGURATION_DEP) -r:PrebuiltSystem=$(topdir)/class/lib/$(PROFILE)/System.dll
$(the_lib): $(CONFIGURATION_DEP_FILE)
endif

$(test_lib): $(test_lib).config $(TEST_RESOURCES)

$(test_lib).config: Test/test-config-file
	cp $< $@

CLEAN_FILES = $(test_lib).config

# run the PKITS tests only if the data was installed/activated, otherwise ignore them
ifeq (net_2_0, $(PROFILE))
pkits_files := $(wildcard Test/System.Security.Cryptography.X509Certificates/pkits/hint)
ifndef pkits_files
TEST_HARNESS_EXCLUDES = /exclude:NotWorking,ValueAdd,CAS,InetAccess,PKITS
TEST_HARNESS_EXCLUDES_ONDOTNET = /exclude:NotDotNet,CAS,PKITS
endif
endif
