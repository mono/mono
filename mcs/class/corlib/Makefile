thisdir = class/corlib
SUBDIRS =
include ../../build/rules.make

real_corlib := $(corlib)

# corlib is crazy to build so we skip build/library.make and do stuff
# ourselves.
#
# Here, we define a bunch of variables.

corlib = $(topdir)/class/lib/corlib.dll
testlib = corlib_test.dll
cmplib = $(topdir)/class/lib/corlib_cmp.dll

sourcefile = corlib.dll.sources
response = $(depsdir)/corlib.dll.response
makefrag = $(depsdir)/corlib.dll.makefrag
stampfile = $(depsdir)/corlib.dll.stamp
corlib_flags = /unsafe /nostdlib

test_sourcefile = corlib_test.dll.sources
test_response = $(depsdir)/$(testlib).response
test_makefrag = $(depsdir)/$(testlib).makefrag
test_stampfile = $(depsdir)/$(testlib).stamp
test_flags = /nowarn:0618 /nowarn:0672 /r:$(corlib) /r:$(topdir)/class/lib/NUnit.Framework.dll

cmp_response = $(depsdir)/corlib_cmp.dll.response
cmp_makefrag = $(depsdir)/corlib_cmp.dll.makefrag
cmp_stampfile = $(depsdir)/corlib_cmp.dll.stamp
cmp_flags = /r:$(real_corlib) $(corlib_flags)

# Here, we make all.

all-local: $(corlib)

# should we install cmplib? sure!

install-local: $(corlib)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(corlib) $(DESTDIR)$(prefix)/lib

clean-local:
	-rm -f $(corlib) $(cmplib) $(testlib) TestResult.xml \
	       $(response) $(makefrag) $(stampfile) \
	       $(test_response) $(test_makefrag) $(test_stampfile) \
	       $(cmp_response) $(cmp_makefrag) $(cmp_stampfile)


test-local: $(corlib) $(testlib)

run-test-local:
	$(TEST_RUNTIME) $(TEST_HARNESS) $(testlib)

# OK, that was the easy part. Now we get to the fun bits.
#
# first, make dist.

DISTFILES = $(sourcefile) $(test_sourcefile) \
	corlib.dll.win32-excludes corlib_cmp.dll.excludes Test/ms_run_test.sh

# see $(topdir)/build/library.make for explanation of what's going on
# here.

dist-local: dist-default $(test_response)
	cat $(sourcefile) $(test_response) |xargs -n 20 \
	    $(SHELL) -c 'for f in $$* ; do \
	        dest=`dirname $(distdir)/$$f` ; \
	        $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	    done' dollar0

# plain ol corlib

$(corlib): $(makefrag) $(response) $(stampfile)
	$(CSCOMPILE) $(LIBRARY_FLAGS) $(corlib_flags) /target:library /out:$@ @$(response)

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
	$(CSCOMPILE) $(LIBRARY_FLAGS) $(cmp_flags) /target:library /out:$@ @$(cmp_response)

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

-include $(cmp_makefrag)

ifndef HAVE_CMP_MAKEFRAG
$(cmp_stampfile):
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
