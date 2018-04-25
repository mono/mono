thisdir = tools/monop
SUBDIRS = 
include ../../build/rules.make

PROGRAM = monop.exe
LOCAL_MCS_FLAGS += -d:NO_AUTHENTICODE,STATIC,NO_SYMBOL_WRITER
LIB_REFS = System

CLEAN_FILES = monop.exe monop2.exe *.mdb

include ../../build/executable.make

run-test-local : basic-tests

basic-tests:
	for type in System.Array System.String 'System.Collections.Generic.List`1'; do \
	echo $$type; MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(build_lib) $$type >/dev/null || exit 1; done
