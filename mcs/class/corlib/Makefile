thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

# corlib is crazy to build so we skip build/library.make and do stuff
# ourselves.
#
# Here, we define a bunch of variables.

corlib_flags = /unsafe /nostdlib
LOCAL_MCS_FLAGS = /nowarn:649 /nowarn:169 -d:INSIDE_CORLIB

LIBRARY = corlib.dll
LIBRARY_NAME = mscorlib.dll
LIB_MCS_FLAGS = $(corlib_flags)
LIBRARY_USE_INTERMEDIATE_FILE = yes

# disable, until people fix their code...
#corlib_flags = /unsafe /nostdlib /d:INSIDE_CORLIB

LIBRARY_INSTALL_DIR = $(prefix)/lib/mono/$(FRAMEWORK_VERSION)

CLEAN_FILES = $(cmplib) $(reslib) $(plattestlib) $(plattestlib).sources \
	       $(cmp_response) $(cmp_makefrag) \
	       $(res_response) $(res_makefrag) \
	       $(cmppdb) $(respdb) $(plattestpdb)

# If building on a non mono-native platform, default
# to running the unit tests on its corlib, to see if
# our tests are complaint with their (hopefully bug-free)
# corlib. If on a mono-native platform, test our actual
# corlib.
#
# You can access either one on demand with either 'make
# run-monotest' or 'make run-plattest'.

plattestlib = corlib_plattest.dll
plattestpdb = $(patsubst %.dll,%.pdb,$(plattestlib))
reslib = $(topdir)/class/lib/$(PROFILE)/corlib_res.dll
cmplib = $(topdir)/class/lib/$(PROFILE)/corlib_cmp.dll

$(plattestlib).sources: corlib_test.dll.sources $(plattestlib).excludes
	sort corlib_test.dll.sources $(plattestlib).excludes | uniq -u >$@

TEST_MCS_FLAGS = /debug+ /debug:full /nowarn:0618 /nowarn:0672 /unsafe

ifndef PLATFORM_MONO_NATIVE
test_lib = $(plattestlib)
test_against = $(PLATFORM_CORLIB)
HAVE_CS_TESTS = $(plattestlib).sources

## for now, compiling the testsuite with CSC causes CS0583.  So compile with internal MCS
TEST_COMPILE = $(INTERNAL_MCS) $(USE_MCS_FLAGS)
endif

run-test-ondotnet-local: run-plattest-ondotnet

ifdef PLATFORM_MONO_NATIVE
run-monotest: run-test
else
run-monotest:
	$(MAKE) $(reslib)
	$(MAKE) test_lib=corlib_test.dll test_against=$(reslib) run-test

run-plattest:
	$(MAKE) test_lib=$(plattestlib) test_against='$(PLATFORM_CORLIB)' run-test
endif

run-plattest-ondotnet: $(plattestlib)
	$(TEST_HARNESS) /exclude=NotDotNet $(plattestlib)

EXTRA_DISTFILES = \
	corlib.dll.win32-excludes corlib_cmp.dll.excludes corlib_res.dll.excludes \
	$(plattestlib).excludes \
	Test/ms_run_test.sh

LIBRARY_COMPILE = $(BOOT_COMPILE)

include ../../build/library.make

# corlib_cmp

cmppdb = $(patsubst %.dll,%.pdb,$(cmplib))
cmp_response = $(depsdir)/$(PROFILE)_corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/$(PROFILE)_corlib_cmp.dll.makefrag
cmp_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

$(cmplib): $(cmp_makefrag) $(cmp_response)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(cmp_flags) /target:library /out:$@ @$(cmp_response)

$(cmp_response): $(sourcefile) corlib_cmp.dll.excludes
	@echo Creating $@ ...
	@sort $(sourcefile) corlib_cmp.dll.excludes | uniq -u | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(cmp_makefrag): $(cmp_response)
	@echo Creating $@ ...
	@sed 's,^,$(cmplib): ,' $< >$@

# Since we make corlib_cmp on demand, there isn't a real need
# to have full dep tracking for it. Also, the generation of this
# file is busted on Win32 ('sort' seems to mess up line endings),
# leading to a broken build.
#
# -include $(cmp_makefrag)

# corlib_res

respdb = $(patsubst %.dll,%.pdb,$(reslib))
res_response = $(depsdir)/$(PROFILE)_corlib_res.dll.response
res_makefrag = $(depsdir)/$(PROFILE)_corlib_res.dll.makefrag
res_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

$(reslib): $(res_makefrag) $(res_response)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(res_flags) /target:library /out:$@ @$(res_response)

$(res_response): $(sourcefile) corlib_res.dll.excludes
	@echo Creating $@ ...
	@sort $(sourcefile) corlib_res.dll.excludes | uniq -u | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

# warning: embedded tab in the 'echo touch' line
$(res_makefrag): $(res_response)
	@echo Creating $@ ...
	@sed 's,^,$(reslib): ,' $< >$@

-include $(res_makefrag)
