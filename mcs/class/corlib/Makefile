thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make
export __SECURITY_BOOTSTRAP_DB=$(topdir)/class/corlib

# corlib is crazy to build so we skip build/library.make and do stuff
# ourselves.
#
# Here, we define a bunch of variables.

ifdef PLATFORM_MONO_NATIVE
test_against = $(corlib)
else
test_against = $(reslib)
endif

LOCAL_MCS_FLAGS = /nowarn:649 /nowarn:169 -d:INSIDE_CORLIB

gmcs = $(topdir)/gmcs/gmcs.exe

corlib = $(topdir)/class/lib/$(PROFILE)/mscorlib.dll
testlib = corlib_test.dll
cmplib = $(topdir)/class/lib/$(PROFILE)/corlib_cmp.dll
reslib = $(topdir)/class/lib/$(PROFILE)/corlib_res.dll
plattestlib = corlib_plattest.dll

corpdb = $(patsubst %.dll,%.pdb,$(corlib))
testpdb = $(patsubst %.dll,%.pdb,$(testlib))
cmppdb = $(patsubst %.dll,%.pdb,$(cmplib))
respdb = $(patsubst %.dll,%.pdb,$(reslib))
plattestpdb = $(patsubst %.dll,%.pdb,$(plattestlib))

sourcefile = corlib.dll.sources
response = $(depsdir)/$(PROFILE)_corlib.dll.response
makefrag = $(depsdir)/$(PROFILE)_corlib.dll.makefrag
stampfile = $(depsdir)/$(PROFILE)_corlib.dll.stamp
# disable, until people fix their code...
#corlib_flags = /unsafe /nostdlib /d:INSIDE_CORLIB
corlib_flags = /unsafe /nostdlib

net_2_0_bootstrap_response = $(depsdir)/corlib_net_2_0_bootstrap.dll.response
net_2_0_bootstrap_makefrag = $(depsdir)/corlib_net_2_0_bootstrap.dll.makefrag
net_2_0_bootstrap_stampfile = $(depsdir)/corlib_net_2_0_bootstrap.dll.stamp

net_2_0_response = $(depsdir)/corlib_net_2_0.dll.response
net_2_0_makefrag = $(depsdir)/corlib_net_2_0.dll.makefrag
net_2_0_stampfile = $(depsdir)/corlib_net_2_0.dll.stamp

test_sourcefile = corlib_test.dll.sources
test_response = $(depsdir)/$(PROFILE)_$(testlib).response
test_makefrag = $(depsdir)/$(PROFILE)_$(testlib).makefrag
test_stampfile = $(depsdir)/$(PROFILE)_$(testlib).stamp
test_flags = /nowarn:0618 /nowarn:0672 /unsafe /r:$(test_against) /r:$(topdir)/class/lib/$(PROFILE)/NUnit.Framework.dll

cmp_response = $(depsdir)/$(PROFILE)_corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/$(PROFILE)_corlib_cmp.dll.makefrag
cmp_stampfile = $(depsdir)/$(PROFILE)_corlib_cmp.dll.stamp
cmp_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

res_response = $(depsdir)/$(PROFILE)_corlib_res.dll.response
res_makefrag = $(depsdir)/$(PROFILE)_corlib_res.dll.makefrag
res_stampfile = $(depsdir)/$(PROFILE)_corlib_res.dll.stamp
res_flags = /r:$(PLATFORM_CORLIB) $(corlib_flags)

plattest_excludes = corlib_plattest.dll.excludes
plattest_response = $(depsdir)/$(PROFILE)_$(plattestlib).response
plattest_makefrag = $(depsdir)/$(PROFILE)_$(plattestlib).makefrag
plattest_stampfile = $(depsdir)/$(PROFILE)_$(plattestlib).stamp
plattest_flags = /debug+ /debug:full /nowarn:0618 /nowarn:0672 /unsafe /r:$(PLATFORM_CORLIB) /r:$(topdir)/class/lib/$(PROFILE)/NUnit.Framework.dll

# Here, we make all.

all-local: $(corlib)

install-local: $(corlib) $(makefrag)
	@if test $(PROFILE) = "net_2_0" ; then \
		$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/net_2_0 ; \
		$(INSTALL_LIB) $(corlib) $(DESTDIR)$(prefix)/lib/net_2_0 ; \
	else \
		$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib ; \
		$(INSTALL_LIB) $(corlib) $(DESTDIR)$(prefix)/lib ; \
	fi

uninstall-local:
	@if test $(PROFILE) = "net_2_0" ; then \
		-rm -f $(DESTDIR)$(prefix)/lib/net_2_0/corlib.dll
	else \
		-rm -f $(DESTDIR)$(prefix)/lib/corlib.dll
	fi

clean-local:
	-rm -f $(corlib) $(cmplib) $(testlib) $(reslib) $(plattestlib) TestResult.xml \
	       $(response) $(makefrag) $(stampfile) \
	       $(net_2_0_bootstrap_response) $(net_2_0_bootstrap_makefrag) $(net_2_0_bootstrap_stampfile) \
	       $(net_2_0_response) $(net_2_0_makefrag) $(net_2_0_stampfile) \
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
	@if test $(PROFILE) = "net_2_0" ; then \
		$(MAKE) corlib_net_2_0 || exit 1 ; \
	else \
		$(MAKE) corlib_compile || exit 1 ; \
	fi

corlib_compile: $(makefrag) $(response) $(stampfile) 
	$(BOOT_COMPILE) $(LIBRARY_FLAGS) $(corlib_flags) /target:library /out:../lib/default/mscorlib.dll @$(response)

# A bootstrap corlib 2.0 needs to be built so gmcs can be built and the full net 2.0 corlib can be built
# here we go,....

corlib_net_2_0: corlib_net_2_0_compile

corlib_net_2_0_compile: corlib_net_2_0_bootstrap $(net_2_0_makefrag) $(net_2_0_response) $(net_2_0_stampfile) $(gmcs)
	MONO_PATH="../lib/net_2_0_bootstrap:$(MONO_PATH)" $(RUNTIME) $(gmcs) $(USE_MCS_FLAGS) $(LIBRARY_FLAGS) $(corlib_flags) /define:NET_1_1 /define:NET_2_0 /target:library /out:../lib/net_2_0/mscorlib.dll @$(net_2_0_response)

corlib_net_2_0_bootstrap: $(net_2_0_bootstrap_response) $(net_2_0_bootstrap_stampfile)
	$(BOOTSTRAP_MCS) /nowarn:649 /nowarn:169 /define:INSIDE_CORLIB /define:NET_1_1 /define:BOOTSTRAP_NET_2_0 $(corlib_flags) /target:library /out:../lib/net_2_0_bootstrap/mscorlib.dll @$(net_2_0_bootstrap_response)

$(gmcs):
	cd $(topdir)/gmcs/ && $(MAKE) ;

$(response): $(sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) >$@
endif

# warning: embedded tab in the 'echo touch' line
$(makefrag): $(response)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
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

# warning: embedded tab in the 'echo touch' line
$(cmp_makefrag): $(cmp_response)
	@echo Creating $@ ...
	@echo "HAVE_CMP_MAKEFRAG = yes" >$@.new
	@echo "$(cmp_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
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

# warning: embedded tab in the 'echo touch' line
$(res_makefrag): $(res_response)
	@echo Creating $@ ...
	@echo "HAVE_RES_MAKEFRAG = yes" >$@.new
	@echo "$(res_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
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

# warning: embedded tab in the 'echo touch' line
$(test_makefrag): $(test_response)
	@echo Creating $@ ...
	@echo "HAVE_TEST_MAKEFRAG = yes" >$@.new
	@echo "$(test_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
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
ifdef PLATFORM_MONO_NATIVE
	@cat $(plattest_excludes) $(test_sourcefile) |sort |uniq -u |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $(test_sourcefile) |sort |uniq -u |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif
else
ifdef PLATFORM_MONO_NATIVE
	@cat $(plattest_excludes) $(test_sourcefile) |sort |uniq -u |sed -e 's,^\(.\),Test/\1,' >$@
else
	@cat $(test_sourcefile) |sort |uniq -u |sed -e 's,^\(.\),Test/\1,' >$@
endif
endif

# warning: embedded tab in the 'echo touch' line
$(plattest_makefrag): $(plattest_response)
	@echo Creating $@ ...
	@echo "HAVE_PLATTEST_MAKEFRAG = yes" >$@.new
	@echo "$(plattest_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

-include $(plattest_makefrag)

ifndef HAVE_PLATTEST_MAKEFRAG
$(plattest_stampfile):
	touch $@
endif

#net_2_0_bootstrap

$(net_2_0_bootstrap_response): $(sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) >$@
endif

# warning: embedded tab in the 'echo touch' line
$(net_2_0_boostrap_makefrag): $(net_2_0_boostrap_response)
	@echo Creating $@ ...
	@echo "HAVE_NET_2_0_BOOTSTRAP_MAKEFRAG = yes" >$@.new
	@echo "$(net_2_0_bootstrap_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

-include $(net_2_0_bootstrap_makefrag)

ifndef HAVE_NET_2_0_BOOTSTRAP_MAKEFRAG
$(net_2_0_bootstrap_stampfile):
	touch $@
endif

#net_2_0

$(net_2_0_response): $(sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |$(PLATFORM_TWEAK_CORLIB_SOURCES) >$@
endif

# warning: embedded tab in the 'echo touch' line
$(net_2_0_makefrag): $(net_2_0_response)
	@echo Creating $@ ...
	@echo "HAVE_NET_2_0_MAKEFRAG = yes" >$@.new
	@echo "$(net_2_0_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

-include $(net_2_0_makefrag)

ifndef HAVE_NET_2_0_MAKEFRAG
$(net_2_0_stampfile):
	touch $@
endif

