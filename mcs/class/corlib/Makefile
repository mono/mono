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

# disable, until people fix their code...
#corlib_flags = /unsafe /nostdlib /d:INSIDE_CORLIB

ifeq (net_2_0, $(PROFILE))
LIBRARY_INSTALL_DIR = $(prefix)/lib/mono/2.0
else
LIBRARY_INSTALL_DIR = $(prefix)/lib
endif

CLEAN_FILES = $(cmplib) $(reslib) $(plattestlib) $(plattestlib).sources \
	       $(net20bootstrap_response) $(net20bootstrap_makefrag) \
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

ifndef PLATFORM_MONO_NATIVE
test_lib = $(plattestlib)
test_against = $(PLATFORM_CORLIB)
endif
TEST_MCS_FLAGS = /debug+ /debug:full /nowarn:0618 /nowarn:0672 /unsafe

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
	$(TEST_HARNESS) $(plattestlib)

EXTRA_DISTFILES = \
	corlib.dll.win32-excludes corlib_cmp.dll.excludes corlib_res.dll.excludes \
	$(plattestlib).excludes \
	Test/ms_run_test.sh

ifeq (net_2_0, $(PROFILE))
# Net 2.0 corlib is built with in-tree 'gmcs'
LIBRARY_COMPILE = MONO_PATH="../lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(gmcs) $(USE_MCS_FLAGS) /define:NET_1_1 /define:NET_2_0
else
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

include ../../build/library.make

gmcs = $(topdir)/gmcs/gmcs.exe
net20bootstraplib = $(topdir)/class/lib/net_2_0_bootstrap/mscorlib.dll

ifeq (net_2_0, $(PROFILE))
$(the_lib): $(gmcs)
endif

# The in-tree 'gmcs' needs at least a stripped-down version of the net 2.0 corlib.dll.  We build that first.
# make sure that $(gmcs) is up-to-date
FORCE:
$(gmcs): $(net20bootstraplib) FORCE
	cd $(topdir)/gmcs && $(MAKE)

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
post_process = $(PLATFORM_CHANGE_SEPARATOR_CMD)
else
post_process = cat
endif

#net_2_0_bootstrap

net20bootstrap_response = $(depsdir)/net_2_0_bootstrap_corlib.dll.response
net20bootstrap_makefrag = $(depsdir)/net_2_0_bootstrap_corlib.dll.makefrag

$(net20bootstraplib): $(net20bootstrap_makefrag) $(net20bootstrap_response)
	$(BOOTSTRAP_MCS) /nowarn:649 /nowarn:169 /define:INSIDE_CORLIB /define:NET_1_1 /define:BOOTSTRAP_NET_2_0 $(corlib_flags) /target:library /out:$(net20bootstraplib) @$(net20bootstrap_response)

$(net20bootstrap_response): $(sourcefile)
	@echo Creating $@ ...
	@cat $< |$(post_process) >$@

$(net20bootstrap_makefrag): $(net20bootstrap_response)
	@echo Creating $@ ...
	@sed 's,^,$(net20bootstraplib): ,' $< >$@

-include $(net20bootstrap_makefrag)

# corlib_cmp

cmppdb = $(patsubst %.dll,%.pdb,$(cmplib))
cmp_response = $(depsdir)/$(PROFILE)_corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/$(PROFILE)_corlib_cmp.dll.makefrag
cmp_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

$(cmplib): $(cmp_makefrag) $(cmp_response)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(cmp_flags) /target:library /out:$@ @$(cmp_response)

$(cmp_response): $(response) corlib_cmp.dll.excludes
	@echo Creating $@ ...
	@sort corlib_cmp.dll.excludes $< |uniq -u | $(post_process) >$@

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
	@sort corlib_res.dll.excludes $(sourcefile) |uniq -u | $(post_process) >$@

# warning: embedded tab in the 'echo touch' line
$(res_makefrag): $(res_response)
	@echo Creating $@ ...
	@sed 's,^,$(reslib): ,' $< >$@

-include $(res_makefrag)
