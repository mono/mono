thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make

# corlib is crazy to build so we skip build/library.make and do stuff
# ourselves.
#
# Here, we define a bunch of variables.

ifdef PLATFORM_MONO_NATIVE
test_against = $(corlib)
else
test_against = $(reslib)
endif

corlib = $(topdir)/class/lib/corlib.dll
testlib = corlib_test.dll
cmplib = $(topdir)/class/lib/corlib_cmp.dll
reslib = $(topdir)/class/lib/corlib_res.dll
plattestlib = corlib_plattest.dll

corpdb = $(patsubst %.dll,%.pdb,$(corlib))
testpdb = $(patsubst %.dll,%.pdb,$(testlib))
cmppdb = $(patsubst %.dll,%.pdb,$(cmplib))
respdb = $(patsubst %.dll,%.pdb,$(reslib))
plattestpdb = $(patsubst %.dll,%.pdb,$(plattestlib))

sourcefile = corlib.dll.sources
response = $(depsdir)/corlib.dll.response
makefrag = $(depsdir)/corlib.dll.makefrag
stampfile = $(depsdir)/corlib.dll.stamp
# disable, until people fix their code...
#corlib_flags = /unsafe /nostdlib /d:INSIDE_CORLIB
corlib_flags = /unsafe /nostdlib

test_sourcefile = corlib_test.dll.sources
test_response = $(depsdir)/$(testlib).response
test_makefrag = $(depsdir)/$(testlib).makefrag
test_stampfile = $(depsdir)/$(testlib).stamp
test_flags = /nowarn:0618 /nowarn:0672 /r:$(test_against) /r:$(topdir)/class/lib/NUnit.Framework.dll

cmp_response = $(depsdir)/corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/corlib_cmp.dll.makefrag
cmp_stampfile = $(depsdir)/corlib_cmp.dll.stamp
cmp_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

res_response = $(depsdir)/corlib_res.dll.response
res_makefrag = $(depsdir)/corlib_res.dll.makefrag
res_stampfile = $(depsdir)/corlib_res.dll.stamp
res_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

plattest_excludes = corlib_plattest.dll.excludes
plattest_response = $(depsdir)/$(plattestlib).response
plattest_makefrag = $(depsdir)/$(plattestlib).makefrag
plattest_stampfile = $(depsdir)/$(plattestlib).stamp
plattest_flags = /nowarn:0618 /nowarn:0672 /r:$(PLATFORM_CORLIB) /r:$(topdir)/class/lib/NUnit.Framework.dll

# Here, we make all.

all-local: $(corlib)

install-local: $(corlib)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(corlib) $(DESTDIR)$(prefix)/lib

clean-local:
	-rm -f $(corlib) $(cmplib) $(testlib) $(reslib) TestResult.xml \
	       $(response) $(makefrag) $(stampfile) \
	       $(test_response) $(test_makefrag) $(test_stampfile) \
	       $(cmp_response) $(cmp_makefrag) $(cmp_stampfile) \
	       $(res_response) $(res_makefrag) $(res_stampfile) \
	       $(plattest_response) $(plattest_makefrag) $(plattest_stampfile) \
	       $(corpdb) $(cmppdb) $(testpdb) $(respdb) $(plattestpdb)


# If building on a non mono-native platform, default
# to running the unit tests on its corlib, to see if
# our tests are complaint with their (hopefully bug-free)
# corlib. If on a mono-native platform, test our actual
# corlib.
#
# You can access either one on demand with either 'make
# run-monotest' or 'make run-plattest'.

ifdef PLATFORM_MONO_NATIVE
test-local: $(test_against) $(testlib)

run-test-local: run-monotest
else
test-local: $(plattestlib)

run-test-local: run-plattest
endif

run-monotest: $(test_against) $(testlib)
	$(TEST_RUNTIME) $(TEST_HARNESS) $(testlib)

run-plattest: $(plattestlib)
	$(TEST_RUNTIME) $(TEST_HARNESS) $(plattestlib)

# OK, that was the easy part. Now we get to the fun bits.
#
# first, make dist.

DISTFILES = $(sourcefile) $(test_sourcefile) \
	corlib.dll.win32-excludes corlib_cmp.dll.excludes corlib_res.dll.excludes \
	$(plattest_excludes) \
	Test/ms_run_test.sh

# see $(topdir)/build/library.make for explanation of what's going on
# here.

dist-local: dist-default
	for f in `cat $(sourcefile)` `cat $(test_sourcefile) |sed -e 's,^\(.\),Test/\1,'` ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

# plain ol corlib

$(corlib): $(makefrag) $(response) $(stampfile)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(corlib_flags) /target:library /out:$@ @$(response)

$(response): $(sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) >$@
endif

$(makefrag): $(response)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@$(ECHO_ESCAPE) "\ttouch \$$@" >>$@
	@rm -rf $@.new

-include $(makefrag)

ifndef HAVE_MAKEFRAG
$(stampfile):
	touch $@
endif

# corlib_cmp

$(cmplib): $(cmp_makefrag) $(cmp_response) $(cmp_stampfile)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(cmp_flags) /target:library /out:$@ @$(cmp_response)

$(cmp_response): $(response) corlib_cmp.dll.excludes
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat corlib_cmp.dll.excludes |$(PLATFORM_CHANGE_SEPARATOR_CMD) |cat - $< |sort |uniq -u >$@
else
	@cat corlib_cmp.dll.excludes |cat - $< |sort |uniq -u >$@
endif

$(cmp_makefrag): $(cmp_response)
	@echo Creating $@ ...
	@echo "HAVE_CMP_MAKEFRAG = yes" >$@.new
	@echo "$(cmp_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@$(ECHO_ESCAPE) "\ttouch \$$@" >>$@
	@rm -rf $@.new

# Since we make corlib_cmp on demand, there isn't a real need
# to have full dep tracking for it. Also, the generation of this
# file is busted on Win32 ('sort' seems to mess up line endings),
# leading to a broken build.
#
# -include $(cmp_makefrag)

ifndef HAVE_CMP_MAKEFRAG
$(cmp_stampfile):
	touch $@
endif

# corlib_res

$(reslib): $(res_makefrag) $(res_response) $(res_stampfile)
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(res_flags) /target:library /out:$@ @$(res_response)

$(res_response): $(sourcefile) corlib_res.dll.excludes
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat corlib_res.dll.excludes $(sourcefile) |sort |uniq -u |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat corlib_res.dll.excludes $(sourcefile) |sort |uniq -u >$@
endif

$(res_makefrag): $(res_response)
	@echo Creating $@ ...
	@echo "HAVE_RES_MAKEFRAG = yes" >$@.new
	@echo "$(res_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@$(ECHO_ESCAPE) "\ttouch \$$@" >>$@
	@rm -rf $@.new

-include $(res_makefrag)

ifndef HAVE_RES_MAKEFRAG
$(res_stampfile):
	touch $@
endif

# corlib_test

$(testlib): $(test_makefrag) $(test_response) $(test_stampfile)
	$(CSCOMPILE) $(test_flags) /target:library /out:$@ @$(test_response)

$(test_response): $(test_sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |sed -e 's,^\(.\),Test/\1,' >$@
endif

$(test_makefrag): $(test_response)
	@echo Creating $@ ...
	@echo "HAVE_TEST_MAKEFRAG = yes" >$@.new
	@echo "$(test_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@$(ECHO_ESCAPE) "\ttouch \$$@" >>$@
	@rm -rf $@.new

-include $(test_makefrag)

ifndef HAVE_TEST_MAKEFRAG
$(test_stampfile):
	touch $@
endif

# corlib_plattest

$(plattestlib): $(plattest_makefrag) $(plattest_response) $(plattest_stampfile)
	$(CSCOMPILE) $(plattest_flags) /target:library /out:$@ @$(plattest_response)

$(plattest_response): $(test_sourcefile) $(plattest_excludes)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $(plattest_excludes) $(test_sourcefile) |sort |uniq -u |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $(plattest_excludes) $(test_sourcefile) |sort |uniq -u |sed -e 's,^\(.\),Test/\1,' >$@
endif

$(plattest_makefrag): $(plattest_response)
	@echo Creating $@ ...
	@echo "HAVE_PLATTEST_MAKEFRAG = yes" >$@.new
	@echo "$(plattest_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@$(ECHO_ESCAPE) "\ttouch \$$@" >>$@
	@rm -rf $@.new

-include $(plattest_makefrag)

ifndef HAVE_PLATTEST_MAKEFRAG
$(plattest_stampfile):
	touch $@
endif
