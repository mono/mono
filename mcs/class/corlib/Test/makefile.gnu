topdir = ../../..

LIBRARY = corlib_test.dll

LIB_LIST = corlib_test.args
LIB_FLAGS =	\
		-r $(topdir)/class/lib/corlib.dll \
		-r $(topdir)/class/lib/System.dll \
	    -r $(topdir)/nunit20/NUnit.Framework.dll

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=_DUMMY_

include $(topdir)/class/library.make

MCS_FLAGS = --target library --noconfig

NUNITCONSOLE=$(topdir)/nunit20/nunit-console.exe
MONO_PATH = $(topdir)/nunit20:.

test: $(LIBRARY) run_test

.PHONY: run_test

run_test:
	-MONO_PATH=$(MONO_PATH) mono $(NUNITCONSOLE) corlib_test.dll
