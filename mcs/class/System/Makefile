thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll

# Because System.dll and Syste.Xml.dll have cyclic dependency we need two-pass build.
# 1st pass - build System.dll without System.Xml.dll reference
# 2nd pass - build System.dll with System.Xml.dll reference

LIB_MCS_FLAGS = /r:$(corlib)
TEST_MCS_FLAGS = /nowarn:1595 /nowarn:0618

CYCLIC_DEP = System.Xml.dll

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file

ifdef SECOND_PASS
	include ../../build/library.make

ifdef HAVE_SYSTEM_XML

LIB_MCS_FLAGS += /define:XML_DEP /r:$(CYCLIC_DEP)

$(the_lib): ../lib/$(PROFILE)/$(CYCLIC_DEP)

else

# First pass:
#   1. Build temporary System.dll without depending on System.Xml.dll
#   2. Since mcs.exe depends on System.dll, use BOOT_COMPILE to compile System.dll
LIBRARY_COMPILE = $(BOOT_COMPILE)

endif

else

test-local: all-local

all-local install-local test-local run-test-local clean-local: 
	@set -e; \
	if test ! -f ../lib/$(PROFILE)/$(CYCLIC_DEP) ; then \
	    echo "Creating temporary $(LIBRARY) without $(CYCLIC_DEP) reference." ; \
	    $(MAKE) SECOND_PASS=yes $@ ; \
	    rm -f '$(depsdir)/$(PROFILE)_$(LIBRARY).stamp'; \
	else \
	    $(MAKE) SECOND_PASS=yes HAVE_SYSTEM_XML=yes $@ ; \
	fi

endif
