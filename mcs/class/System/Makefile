thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

# Because System.dll and System.Xml.dll have cyclic dependency we need two-pass build.
# 1st pass - build System.dll without System.Xml.dll reference
# 2nd pass - build System.dll with System.Xml.dll reference

LIB_MCS_FLAGS = /r:$(corlib)
TEST_MCS_FLAGS = /nowarn:1595 /nowarn:0618

CYCLIC_DEP := System.Xml.dll
CYCLIC_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(CYCLIC_DEP))

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

ifndef CYCLIC_DEP_FILE
# First pass: Build temporary System.dll without depending on System.Xml.dll

NO_SIGN_ASSEMBLY = yes
all-local: echo-warning
.PHONY: echo-warning
echo-warning:
	@echo "** Warning: System.dll built without parts that depend on System.Xml.dll"
endif

include ../../build/library.make

ifdef CYCLIC_DEP_FILE
LIB_MCS_FLAGS += /define:XML_DEP /r:$(CYCLIC_DEP)
$(the_lib): $(CYCLIC_DEP_FILE)
endif
