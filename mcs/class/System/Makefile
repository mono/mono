thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll

# Because System.dll and Syste.Xml.dll have cyclic dependency we need two-pass build.
# 1st pass - build System.dll without System.Xml.dll reference
# 2nd pass - build System.dll with System.Xml.dll reference

LIB_MCS_FLAGS = /r:$(corlib) $(EXTRA_FLAGS) /define:XML_DEP /r:System.Xml.dll
TEST_MCS_FLAGS = /nowarn:1595 /nowarn:0618

CYCLIC_DEP = System.Xml.dll

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file

ifdef SECOND_PASS
	include ../../build/library.make
else
all-local:
	@if ! test -f ../lib/$(CYCLIC_DEP) ; then \
	    echo "Creating temporary $(LIBRARY) without $(CYCLIC_DEP) reference." ; \
	    $(MAKE) SECOND_PASS=yes || exit 1 ; \
	    rm -f '$(depsdir)/$(LIBRARY).stamp'; \
	else \
	    $(MAKE) SECOND_PASS=yes EXTRA_FLAGS='/define:XML_DEP /r:$(CYCLIC_DEP)' || exit 1; \
	fi

## TODO: how to do this without duplication

install-local:
	    @$(MAKE) SECOND_PASS=yes EXTRA_FLAGS='/r:$(CYCLIC_DEP)' install-local || exit 1; \
	    
test-local: all-local
	    @$(MAKE) SECOND_PASS=yes EXTRA_FLAGS='/r:$(CYCLIC_DEP)' test-local || exit 1; \
	    
run-test-local:
	    @$(MAKE) SECOND_PASS=yes EXTRA_FLAGS='/r:$(CYCLIC_DEP)' run-test-local || exit 1; \
	    
clean-local:
	    @$(MAKE) SECOND_PASS=yes EXTRA_FLAGS='/r:$(CYCLIC_DEP)' clean-local || exit 1; \
	    
endif
