# -*- makefile -*-
#
# The rules for building our class libraries.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

# All the dep files now land in the same directory so we
# munge in the library name to keep the files from clashing.

sourcefile = $(LIBRARY).sources
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
response = $(depsdir)/$(LIBRARY).response
else
response = $(sourcefile)
endif
makefrag = $(depsdir)/$(LIBRARY).makefrag
stampfile = $(depsdir)/$(LIBRARY).stamp
the_lib = $(topdir)/class/lib/$(LIBRARY)

ifndef NO_TEST
test_lib = $(patsubst %.dll,%_test.dll,$(LIBRARY))
test_sourcefile = $(test_lib).sources
test_response = $(depsdir)/$(test_lib).response
test_makefrag = $(depsdir)/$(test_lib).makefrag
test_stampfile = $(depsdir)/$(test_lib).stamp
test_flags = /r:$(the_lib) /r:$(topdir)/class/lib/NUnit.Framework.dll $(TEST_MCS_FLAGS)
endif

all-local: $(the_lib)

install-local: $(the_lib)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(prefix)/lib

clean-local:
	-rm -f $(the_lib) $(makefrag) $(test_lib) \
	       $(test_makefrag) $(test_response) \
	       $(stampfile) $(test_stampfile) \
	       TestResult.xml
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -rf $(response)
endif

ifndef NO_TEST
test-local: $(the_lib) $(test_lib)

run-test-local:
	$(TEST_RUNTIME) $(TEST_HARNESS) $(test_lib)

else
test-local: $(the_lib)

run-test-local:
endif

DISTFILES = $(sourcefile) $(test_sourcefile) $(EXTRA_DISTFILES)

# just in case you ever wanted to know how to copy a list of 800
# files without excessively long commandlines...
#
# We need dollar0 because $(SHELL) -c interprets arguments as 
#
#  $(SHELL) -c 'the script' $0 $1 $2 ....
#
# Ideally we wouldn't use $(test_response) (it'd be nice to make dist
# with an unbuilt tree), but the -include lines generate the makefrags
# anyway, so we might as well use them.

dist-local: dist-default $(test_response)
	cat $(sourcefile) $(test_response) |xargs -n 20 \
	    $(SHELL) -c 'for f in $$* ; do \
	        dest=`dirname $(distdir)/$$f` ; \
	        $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	    done' dollar0

# Fun with dependency tracking

$(the_lib): $(makefrag) $(stampfile) $(response)
	$(CSCOMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$@ @$(response)

$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo -e "\ttouch \$$@" >>$@
	@rm -rf $@.new

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
$(response): $(sourcefile)
	cat $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

ifndef HAVE_MAKEFRAG
$(stampfile):
	touch $@
endif

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

ifndef NO_TEST
$(test_lib): $(test_makefrag) $(the_lib) $(test_response) $(test_stampfile)
	$(CSCOMPILE) /target:library /out:$@ $(test_flags) @$(test_response)

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
	@echo -e "\ttouch \$$@" >>$@
	@rm -rf $@.new

-include $(test_makefrag)
endif

ifndef HAVE_TEST_MAKEFRAG
$(test_stampfile):
	touch $@
endif

