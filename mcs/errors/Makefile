thisdir = errors
SUBDIRS = 
include ../build/rules.make

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

ifeq (net_1_1, $(PROFILE))
# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = $(with_mono_path) $(INTERNAL_MCS)
else
EXTRA_SUPPORT_FILES = CSFriendAssembly-lib.dll GCS0122-2-lib.dll GCS0730-lib.dll GCS0733-lib.dll GCS1685-lib.dll
endif

ilasm = $(topdir)/class/lib/$(PROFILE)/ilasm.exe
ILASM = $(with_mono_path) $(RUNTIME) $(RUNTIME_FLAGS) $(ilasm)

DISTFILES = \
	CONTRIBUTORS_README 	\
	errors.txt 		\
	do-tests.pl		\
	$(wildcard *.cs)	\
	$(wildcard *.il)	\
	$(wildcard *.inc)	\
	$(wildcard *.pub)	\
	$(wildcard *.snk)	\
	$(wildcard *.response)	\
	$(wildcard known-issues-*)

TEST_SUPPORT_FILES = \
	CS0118-2-lib.dll CS0122-10-lib.dll CS0122-14-lib.dll CS0122-15-lib.dll CS0122-19-lib.dll CS0144-3-lib.dll \
	CS0205-3-lib.dll \
	CS0229-3-lib.dll CS0229-4-lib.dll \
	CS0433-lib.dll CS0433-2-lib.dll \
	CS0534-3-lib.dll CS0534-4-lib.dll CS0571-3-lib.dll CS0571-5-lib.dll CS0571-6-lib.dll \
	CS0612-2-lib.dll CS0618-2-lib.dll CS0619-8-lib.dll CS0619-17-lib.dll CS0619-32-lib.dll CS0619-33-lib.dll CS0619-36-lib.dll CS0619-42-lib.dll \
	CS0619-43-lib.dll CS1546-lib.dll CS0619-51-lib.dll CS1509-module.dll CS1681-2-lib.dll \
	CS2015-lib.dll CS3005-16-lib.dll CS3013-module.dll CSExternAlias-lib.dll \
	$(EXTRA_SUPPORT_FILES) 

# mention all targets
all-local $(STD_TARGETS:=-local):

VALID_PROFILE := $(filter net_1_1 net_2_0 net_2_1 net_4_0, $(PROFILE))
ifdef VALID_PROFILE

check: run-mcs-tests 

# again, run-test is when the tests actually happen, so
# don't compile on make test.

test-local:
	@:

run-test-local: clean-local
	$(MAKE) run-mcs-tests

test-everything:
	$(MAKE) PROFILE=net_1_1 run-test
	$(MAKE) PROFILE=net_2_0 run-test

ifeq (net_4_0, $(PROFILE))
COMPILER_NAME = dmcs
TEST_PATTERN = '*cs*.cs'
endif
ifeq (net_2_1, $(PROFILE))
COMPILER_NAME = smcs
TEST_PATTERN = '*cs*.cs'
LOCAL_RUNTIME_FLAGS = --security=temporary-smcs-hack
endif
ifeq (net_2_0, $(PROFILE))
COMPILER_NAME = gmcs
TEST_PATTERN = '*cs*.cs'
endif
ifeq (net_1_1, $(PROFILE))
COMPILER_NAME = mcs
TEST_PATTERN = 'cs*.cs'
endif

COMPILER = $(topdir)/class/lib/$(PROFILE)/$(COMPILER_NAME).exe
TESTER = MONO_RUNTIME='$(RUNTIME)' $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(LOCAL_RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/compiler-tester.exe

run-mcs-tests: $(TEST_SUPPORT_FILES)
	$(TESTER) -mode:neg -files:$(TEST_PATTERN) -compiler:$(COMPILER) -issues:known-issues-$(COMPILER_NAME) -log:$(COMPILER_NAME).log $(TESTER_OPTIONS)

cecil:
	$(TESTER) -mode:neg -files:$(TEST_PATTERN) -compiler:../tests/gmcs.exe -issues:known-issues-$(COMPILER_NAME) -log:$(COMPILER_NAME).log $(TESTER_OPTIONS) -verbose

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

endif
