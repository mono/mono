#
# use `run-test'
#

thisdir = tests
SUBDIRS =
include ../build/rules.make

DISTFILES = README.tests $(wildcard *.cs) $(wildcard *.il) $(wildcard *.xml) $(wildcard *.inc) $(wildcard known-issues-*)

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

ifeq (default, $(PROFILE))
# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.
MCS = $(with_mono_path) $(INTERNAL_MCS)
endif
ilasm = $(topdir)/class/lib/net_1_1_bootstrap/ilasm.exe
ILASM = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(ilasm)

ifeq (net_2_0, $(PROFILE))
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_2_0_bootstrap/mcs.exe
endif

USE_MCS_FLAGS :=

all-local install-local uninstall-local:

# casts

bootstrap-cast.exe: gen-cast-test.cs
	$(BOOT_COMPILE) -target:exe /out:$@ $<

casts.cs: bootstrap-cast.exe
	$(with_mono_path) $(RUNTIME) $(RUNTIME_FLAGS) $< >$@

casts-mcs.exe: casts.cs
	$(CSCOMPILE) -target:exe /out:$@ $<

casts-boot.exe: casts.cs
	$(BOOT_COMPILE) -target:exe /out:$@ $<

boot-casts.out: casts-boot.exe
	$(with_mono_path) $(RUNTIME) $(RUNTIME_FLAGS) $< >$@

mcs-casts.out: casts-mcs.exe
	$(with_mono_path) $(RUNTIME) $(RUNTIME_FLAGS) $< >$@

test-casts: boot-casts.out mcs-casts.out
	cmp $^

test-local: casts-boot.exe

ifeq (net_2_0, $(PROFILE))
COMPILER_NAME = gmcs
COMPILER = $(topdir)/gmcs/gmcs.exe
TEST_PATTERN = '*test-*.cs'
else
COMPILER_NAME = mcs
COMPILER = $(topdir)/class/lib/$(PROFILE)/mcs.exe
TEST_PATTERN = 'test-*.cs'
endif

run-test-local: test-377-lib.dll test-443-lib.dll
	MONO_RUNTIME='$(RUNTIME)' $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/compiler-tester.exe positive $(TEST_PATTERN) $(COMPILER) known-issues-$(COMPILER_NAME) $(COMPILER_NAME).log

# do nothing for this target
run-test-ondotnet-local:

test-everything:
	$(MAKE) PROFILE=default run-test
	$(MAKE) PROFILE=net_2_0 run-test

test-generics:
	$(MAKE) PROFILE=net_2_0 run-test

clean-local:
	-rm -fr dir-*
	-rm -f *.exe *.dll *.netmodule *.out *.pdb casts.cs *.log
	-rm -f xml-*.xml

dist-local: dist-default
	rm -f $(distdir)/casts.cs

%-il.dll: %-il.il
	$(ILASM) /dll $<

%-lib.dll: %-lib.il
	$(ILASM) /dll /out:$@ $<

ifeq (default, $(PROFILE))
run-test-local: ilasm
ilasm:
	$(ILASM) /dll property-il.il
	$(CSCOMPILE) /r:property-il.dll property-main.cs /out:property-main.exe
	$(TEST_RUNTIME) property-main.exe
endif
