thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll

# Because System.dll and Syste.Xml.dll have cyclic dependency we need two-pass build.
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

ifdef USE_BOOT_COMPILE
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

include ../../build/library.make

ifdef CYCLIC_DEP_FILE
LIB_MCS_FLAGS += /define:XML_DEP /r:$(CYCLIC_DEP)
$(the_lib): $(CYCLIC_DEP_FILE)
else
# First pass: Build temporary System.dll without depending on System.Xml.dll
$(the_lib): echo-warning
.PHONY: echo-warning
echo-warning:
	@echo "** Building temporary System.dll without parts that depend on System.Xml"
endif
