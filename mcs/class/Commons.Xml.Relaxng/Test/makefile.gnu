topdir = ../../..

LIBRARY = Commons.Xml.Relaxng_test.dll

LIB_LIST = Commons.Xml.Relaxng_test.args
LIB_FLAGS =	\
		-r $(topdir)/class/lib/corlib.dll \
		-r $(topdir)/class/lib/System.dll \
		-r $(topdir)/class/lib/System.Xml.dll \
		-r $(topdir)/class/lib/Commons.Xml.Relaxng.dll \
	    	-r $(topdir)/class/lib/NUnit.Framework.dll

ifdef SUBDIR
USE_SOURCE_RULES=1
SOURCES_INCLUDE=./$(SUBDIR)/*.cs
SOURCES_EXCLUDE=_DUMMY_
endif

include $(topdir)/class/library.make

NUNITCONSOLE=$(topdir)/nunit20/nunit-console.exe
MONO_PATH = $(topdir)/nunit20:$(topdir)/class/lib:.

test: $(LIBRARY) run_test

.PHONY: run_test

run_test:
	-MONO_PATH=$(MONO_PATH) mono --debug $(NUNITCONSOLE) $(LIBRARY)
