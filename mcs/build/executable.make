# -*- makefile -*-
#
# The rules for building a program.

# I'd rather not create a response file here,
# but since on Win32 we need to munge the paths
# anyway, we might as well.

sourcefile = $(PROGRAM).sources
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
response = $(depsdir)/$(PROGRAM).response
else
response = $(sourcefile)
endif
stampfile = $(depsdir)/$(PROGRAM).stamp
makefrag = $(depsdir)/$(PROGRAM).makefrag

all-local: $(PROGRAM)

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(prefix)/bin

clean-local:
	-rm -f *.exe $(BUILT_SOURCES) $(CLEAN_FILES) $(stampfile) $(makefrag)
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -f $(response)
endif

ifndef HAS_TEST
test-local: $(PROGRAM)

run-test-local:
endif

DISTFILES = $(sourcefile) $(EXTRA_DISTFILES)

dist-local: dist-default
	cat $(sourcefile) |xargs -n 20 \
	    $(SHELL) -c 'for f in $$* ; do \
	        dest=`dirname $(distdir)/$$f` ; \
	        $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	    done' dollar0

# Changing makefile probably means changing the
# sources, so let's be safe and add a Makefile dep

$(PROGRAM): $(makefrag) $(response) $(stampfile)
	$(CSCOMPILE) /target:exe /out:$@ $(BUILT_SOURCES) @$(response)

$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): $(BUILT_SOURCES) \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@$(ECHO_ESCAPE) "\ttouch \$$@" >>$@
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

