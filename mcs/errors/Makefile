thisdir = errors
SUBDIRS = 
include ../build/rules.make

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"
with_mono_path_n11b = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

ifeq (default, $(PROFILE))
# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = $(with_mono_path) $(INTERNAL_MCS)
endif

ilasm = $(topdir)/class/lib/net_1_1_bootstrap/ilasm.exe
ILASM = $(with_mono_path_n11b) $(RUNTIME) $(RUNTIME_FLAGS) $(ilasm)

GENERICS_COMPILE = $(CSCOMPILE) /target:library

DISTFILES = \
	CONTRIBUTORS_README 	\
	errors.txt 		\
	do-tests.pl		\
	$(wildcard *.cs)	\
	$(wildcard *.il)	\
	$(wildcard *.inc)	\
	$(wildcard *.pub)	\
	$(wildcard *.response)	\
	$(wildcard known-issues-*)

TEST_SUPPORT_FILES = \
	CS0118-2-lib.dll CS0122-10-lib.dll CS0122-14-lib.dll CS0122-15-lib.dll CS0433-lib.dll CS0433-2-lib.dll \
	CS0534-3-lib.dll CS0534-4-lib.dll CS0571-3-lib.dll \
	CS0612-2-lib.dll CS0618-2-lib.dll CS0619-8-lib.dll CS0619-17-lib.dll CS0619-32-lib.dll CS0619-33-lib.dll CS0619-36-lib.dll CS0619-42-lib.dll \
	CS0619-43-lib.dll CS1546-lib.dll CS2015-lib.dll CS3005-16-lib.dll CS3013-module.dll CSExternAlias-lib.dll CSFriendAssembly-lib.dll

all-local:

test-local: $(TEST_SUPPORT_FILES)

run-test-ondotnet-local:

# again, run-test is when the tests actually happen, so
# don't compile on make test.

run-test-local: run-mcs-tests 

test-everything:
	$(MAKE) PROFILE=default run-test
	$(MAKE) PROFILE=net_2_0 run-test

ifeq (net_2_0, $(PROFILE))
COMPILER_NAME = gmcs
COMPILER = $(topdir)/gmcs/gmcs.exe
TEST_PATTERN = '*cs*.cs'
else
COMPILER_NAME = mcs
COMPILER = $(topdir)/class/lib/$(PROFILE)/mcs.exe
TEST_PATTERN = 'cs*.cs'
endif

run-mcs-tests: $(TEST_SUPPORT_FILES)

run-mcs-tests:
	MONO_RUNTIME='$(RUNTIME)' $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/compiler-tester.exe negative $(TEST_PATTERN) $(COMPILER) known-issues-$(COMPILER_NAME) $(COMPILER_NAME).log

clean-local:
	rm -f *.exe *.dll *.log *.mdb dummy.xml *.junk

dist-local: dist-default

install-local uninstall-local:

%-lib.dll: %-lib.cs
	$(CSCOMPILE) /target:library /out:$@ $<

%-module.dll: %-module.cs
	$(CSCOMPILE) /target:module /out:$@ $<

%-lib.dll: %-lib.il
	$(ILASM) /dll /out:$@ $<
