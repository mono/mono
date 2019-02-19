#
# use make run-test
#

thisdir = tests
SUBDIRS =
include ../build/rules.make

DISTFILES = $(wildcard dlls/**/*.cs) $(wildcard dlls/*.cs) $(wildcard dlls/*.inc) $(wildcard dlls/*.il)
DISTFILES += $(wildcard *.cs) $(wildcard *.il) $(wildcard *.xml) $(wildcard *.inc) $(wildcard known-issues-*) $(wildcard *.snk)

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

ilasm = $(topdir)/class/lib/$(PROFILE)/ilasm.exe
ILASM = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(ilasm)

-include $(mcs_topdir)/build/config.make

USE_MCS_FLAGS :=

# mention all targets
all-local $(STD_TARGETS:=-local):

VALID_TEST_PROFILE := $(filter net_4_x, $(PROFILE))
ifdef VALID_TEST_PROFILE
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
	-rm -f bootstrap-cast.exe casts.cs casts-boot.exe casts-mcs.exe boot-casts.out mcs-casts.out

TEST_PATTERN = 'v4'
DEFINES = -compiler-options:"-d:NET_4_0;NET_4_5 -debug"

LOCAL_RUNTIME_FLAGS = --verify-all
COMPILER = $(topdir)/class/lib/$(PROFILE)/mcs.exe
TESTER = MONO_RUNTIME='$(RUNTIME)' $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(LOCAL_RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/compiler-tester.exe

TEST_IL := $(wildcard *-lib.il) dlls/test-883.il
TEST_CS := \
	dlls/test-679-2/test-679-lib-2.cs \
	dlls/test-679-1/test-679-lib.cs \
	dlls/test-939-common.cs \
	dlls/test-939-1/test-939-lib.cs \
	dlls/test-939-1/test-939-ref.cs \
	dlls/test-939-2/test-939-lib.cs

build-compiler-lib:
	cd ../class/Mono.CSharp && $(MAKE) NO_DIR_CHECK=yes

qcheck: build-compiler-lib qcheck2

ifdef TEST_WITH_INTERPRETER
KNOWN_ISSUES = known-issues-interp-$(PROFILE)
else
KNOWN_ISSUES = known-issues-$(PROFILE)
endif

qcheck2:
	$(TESTER) -mode:pos -files:$(TEST_PATTERN) -compiler:$(COMPILER) -reference-dir:$(topdir)/class/lib/$(PROFILE) -issues:$(KNOWN_ISSUES) -log:$(PROFILE).log -il:ver-il-$(PROFILE).xml $(DEFINES) $(TOPTIONS)

gen-mt-tests:
	$(TESTER) -mode:nunit -files:'v2' -compiler:$(COMPILER) -reference-dir:$(topdir)/class/lib/$(PROFILE) -issues:known-issues-mt -compiler-options:"-lib:$(topdir)/class/lib/monotouch projects/MonoTouch/ivt.cs"


test-local: $(TEST_IL:.il=.dll) $(TEST_CS:.cs=.dll)

run-test-local: test-local qcheck

check: run-test-local

test-bundle:
	mkdir -p $(TEST_BUNDLE_PATH)/tests/mcs
	cp -L -R dlls $(TEST_BUNDLE_PATH)/tests/mcs/
	cp -L *.cs *.xml *.inc *.dll *.snk $(TEST_BUNDLE_PATH)/tests/mcs/
	cp -L $(topdir)/class/lib/$(PROFILE)/compiler-tester.* $(TEST_BUNDLE_PATH)/tests/mcs/
	cp -L $(KNOWN_ISSUES) $(TEST_BUNDLE_PATH)/tests/mcs/$(KNOWN_ISSUES)
	cp -L ver-il-$(PROFILE).xml $(TEST_BUNDLE_PATH)/tests/mcs/ver-il-$(PROFILE).xml

endif

clean-local:
	-rm -fr dir-*
	-rm -f *.exe *.dll *.netmodule *.out *.pdb *.mdb casts.cs *.log
	-rm -f xml-*.xml

dist-local: dist-default
	rm -f $(distdir)/casts.cs

csproj-local:

%-il.dll: %-il.il
	$(ILASM) /dll $<

%-lib.dll: %-lib.il
	$(ILASM) /dll /out:$@ $<

CSCOMPILE_UTIL = $(CSCOMPILE) -noconfig -nologo -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

dlls/test-679-2/test-679-lib-2.dll: dlls/test-679-2/test-679-lib-2.cs
	$(CSCOMPILE_UTIL) -t:library -out:$@ dlls/test-679-2/test-679-lib-2.cs

dlls/test-679-1/test-679-lib.dll: dlls/test-679-1/test-679-lib.cs dlls/test-679-2/test-679-lib-2.dll
	$(CSCOMPILE_UTIL) -t:library -out:$@ -r:dlls/test-679-2/test-679-lib-2.dll dlls/test-679-1/test-679-lib.cs

dlls/test-939-common.dll: dlls/test-939-common.cs key.snk
	$(CSCOMPILE_UTIL) -t:library -out:$@ dlls/test-939-common.cs -keyfile:key.snk -publicsign

dlls/test-939-1/test-939-lib.dll: dlls/test-939-1/test-939-lib.cs key.snk
	$(CSCOMPILE_UTIL) -t:library -out:$@ dlls/test-939-1/test-939-lib.cs -keyfile:key.snk -publicsign

dlls/test-939-1/test-939-ref.dll: dlls/test-939-1/test-939-ref.cs dlls/test-939-1/test-939-lib.dll key.snk
	$(CSCOMPILE_UTIL) -t:library -out:$@ dlls/test-939-1/test-939-ref.cs -r:dlls/test-939-1/test-939-lib.dll -keyfile:key.snk -publicsign

dlls/test-939-2/test-939-lib.dll: dlls/test-939-2/test-939-lib.cs dlls/test-939-common.dll key.snk
	$(CSCOMPILE_UTIL) -t:library -out:$@ dlls/test-939-2/test-939-lib.cs -r:dlls/test-939-common.dll -keyfile:key.snk -publicsign

dlls/test-883.dll: dlls/test-883.il
	$(ILASM) -dll dlls/test-883.il
