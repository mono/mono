thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

# Because System.dll, System.Security.dll System.Xml.dll have a cyclic dependency, we need a two-pass build.
# 1st pass - build System.dll without System.Xml.dll or System.Security.dll references
# 2nd pass - build System.dll with System.Xml.dll and System.Security.dll references

LIB_MCS_FLAGS = /r:$(corlib)
TEST_MCS_FLAGS = -nowarn:1595 -nowarn:0618 -nowarn:219 -nowarn:67 \
	-resource:Test/System/test-uri-props.txt,test-uri-props.txt \
	-resource:Test/System/test-uri-props-manual.txt,test-uri-props-manual.txt \
	-resource:Test/System/test-uri-relative-props.txt,test-uri-relative-props.txt 

XML_DEP := System.Xml.dll
XML_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(XML_DEP))
CYCLIC_DEPS := $(XML_DEP)
CYCLIC_DEP_FILES := $(XML_DEP_FILE)

ifeq (2.0, $(FRAMEWORK_VERSION))
# Happens on net_2_0_bootstrap and net_2_0 profiles
SECURITY_DEP := System.Security.dll
SECURITY_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(SECURITY_DEP))
CYCLIC_DEPS += $(SECURITY_DEP)
CYCLIC_DEP_FILES += $(SECURITY_DEP_FILE)
endif

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file

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

ifneq ($(CYCLIC_DEPS:%=../lib/$(PROFILE)/%), $(CYCLIC_DEP_FILES))
NO_SIGN_ASSEMBLY = yes
all-local: echo-warning
.PHONY: echo-warning

ifeq   (, $(strip $(CYCLIC_DEP_FILES)))
echo-warning:
	@echo "** Warning: System.dll built without parts that depend on: $(CYCLIC_DEPS)"
else     # defined CYCLIC_DEP_FILES
ifdef      SECURITY_DEP_FILE
echo-warning:
	@echo "** Warning: built System.dll with 'System.Security.dll'-dependent parts"
	@echo "** Warning: ... without 'System.Xml.dll' present: it may not be usable"

else
echo-warning:
	@echo "** Warning: System.dll built without parts that depend on System.Security.dll"
endif
endif
endif

include ../../build/library.make

ifdef XML_DEP_FILE
LIB_MCS_FLAGS += /define:XML_DEP /r:$(XML_DEP)
$(the_lib): $(XML_DEP_FILE)
endif

ifdef SECURITY_DEP_FILE
LIB_MCS_FLAGS += /define:SECURITY_DEP /r:$(SECURITY_DEP)
$(the_lib): $(SECURITY_DEP_FILE)
endif

$(test_lib): $(test_lib).config Test/System/test-uri-props.txt Test/System/test-uri-props-manual.txt Test/System/test-uri-relative-props.txt

$(test_lib).config: Test/test-config-file
	cp $< $@

CLEAN_FILES = $(test_lib).config
