topdir = ../../..

LIBRARY = data_linux_test.dll

LIB_LIST = data_linux_test.args
LIB_FLAGS =	\
		-r $(topdir)/class/lib/corlib.dll \
		-r $(topdir)/class/lib/System.Data.dll \
		-r $(topdir)/class/lib/System.Xml.dll \
		-r $(topdir)/class/lib/System.dll \
        	-r $(topdir)/nunit20/NUnit.Framework.dll

ifdef SUBDIR
USE_SOURCE_RULES=1
SOURCES_INCLUDE=./$(SUBDIR)/*.cs
SOURCES_EXCLUDE=_DUMMY_
endif

include $(topdir)/class/library.make

NUNITCONSOLE=$(topdir)/nunit20/nunit-console.exe
MONO_PATH = $(topdir)/nunit20:.

test: $(LIBRARY) run_test

.PHONY: run_test

run_test:
	-MONO_PATH=$(MONO_PATH) mono --debug $(NUNITCONSOLE) $(LIBRARY)
