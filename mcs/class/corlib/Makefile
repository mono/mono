thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

RESOURCE_FILES = \
	resources/collation.core.bin \
	resources/collation.tailoring.bin \
	resources/collation.cjkCHS.bin \
	resources/collation.cjkCHT.bin \
	resources/collation.cjkJA.bin \
	resources/collation.cjkKO.bin \
	resources/collation.cjkKOlv2.bin

corlib_flags = -unsafe -nostdlib
LOCAL_MCS_FLAGS = -nowarn:169,612,618,649 -d:INSIDE_CORLIB

ifneq ($(FRAMEWORK_VERSION),1.0)
LOCAL_MCS_FLAGS += -nowarn:414
endif

LIBRARY = corlib.dll
LIBRARY_NAME = mscorlib.dll
LIB_MCS_FLAGS = $(corlib_flags) $(RESOURCE_FILES:%=/resource:%)
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIBRARY_COMPILE = $(BOOT_COMPILE)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

EXTRA_DISTFILES = \
	corlib.dll.win32-excludes corlib_cmp.dll.excludes corlib_res.dll.excludes \
	$(plattestlib).excludes 		\
	Test/ms_run_test.sh			\
	Test/resources/MyResources.resources	\
	Test/resources/Empty.resources		\
	Test/resources/AFile.txt			\
	$(RESOURCE_FILES)

CLEAN_FILES = $(cmplib) $(reslib) $(plattestverlib) $(plattestverlib).sources \
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
plattestverlib = corlib_plattest_$(PROFILE).dll
plattestpdb = $(patsubst %.dll,%.pdb,$(plattestverlib))
$(plattestverlib).sources: corlib_test.dll.sources $(plattestlib).excludes
	sort corlib_test.dll.sources $(plattestlib).excludes | uniq -u >$@

TEST_MCS_FLAGS = -debug+ -debug:full -nowarn:168,219,618,672 -unsafe

ifndef PLATFORM_MONO_NATIVE
test_lib = $(plattestverlib)
test_against = $(PLATFORM_CORLIB)
HAVE_CS_TESTS = $(plattestverlib).sources

## for now, compiling the testsuite with CSC causes CS0583.  So compile with internal MCS
TEST_COMPILE = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS) $(USE_MCS_FLAGS)
endif

include ../../build/library.make

ifndef PLATFORM_MONO_NATIVE
run-monotest:
	$(MAKE) $(reslib)
	$(MAKE) test_lib=$(plattestverlib) test_against=$(reslib) run-test

run-plattest:
	$(MAKE) test_lib=$(plattestverlib) test_against='$(PLATFORM_CORLIB)' run-test

else
run-monotest: run-test
endif

# corlib_cmp
cmplib = $(topdir)/class/lib/$(PROFILE)/corlib_cmp.dll
cmppdb = $(cmplib:.dll=.pdb)
cmp_response = $(depsdir)/$(PROFILE)_corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/$(PROFILE)_corlib_cmp.dll.makefrag
cmp_flags = -r:$(PLATFORM_CORLIB) $(corlib_flags)

$(cmplib): $(cmp_makefrag) $(cmp_response)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(cmp_flags) -target:library -out:$@ @$(cmp_response)

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
reslib = $(topdir)/class/lib/$(PROFILE)/corlib_res.dll
respdb = $(reslib:.dll=.pdb)
res_response = $(depsdir)/$(PROFILE)_corlib_res.dll.response
res_makefrag = $(depsdir)/$(PROFILE)_corlib_res.dll.makefrag
res_flags = -r:$(PLATFORM_CORLIB) $(corlib_flags)

$(reslib): $(res_makefrag) $(res_response)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(res_flags) -target:library -out:$@ @$(res_response)

$(res_response): $(sourcefile) corlib_res.dll.excludes
	@echo Creating $@ ...
	@sort $(sourcefile) corlib_res.dll.excludes | uniq -u | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

# warning: embedded tab in the 'echo touch' line
$(res_makefrag): $(res_response)
	@echo Creating $@ ...
	@sed 's,^,$(reslib): ,' $< >$@

-include $(res_makefrag)
