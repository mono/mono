# -*- makefile -*-
#
# The rules for building a program.

# I'd rather not create a response file here,
# but since on Win32 we need to munge the paths
# anyway, we might as well.

base_prog = $(shell basename $(PROGRAM))
sourcefile = $(base_prog).sources
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
response = $(depsdir)/$(base_prog).response
else
response = $(sourcefile)
endif
stampfile = $(depsdir)/$(base_prog).stamp
makefrag = $(depsdir)/$(base_prog).makefrag
pdb = $(patsubst %.exe,%.pdb,$(PROGRAM))

all-local: $(PROGRAM)

install-local: $(PROGRAM)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(prefix)/bin

clean-local:
	-rm -f *.exe $(BUILT_SOURCES) $(CLEAN_FILES) $(pdb) $(stampfile) $(makefrag)
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -f $(response)
endif

ifndef HAS_TEST
test-local: $(PROGRAM)

run-test-local:
endif

DISTFILES = $(sourcefile) $(EXTRA_DISTFILES)

dist-local: dist-default
	for f in `cat $(sourcefile)` ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

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
	@echo Creating $@ ...
	@cat $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

ifndef HAVE_MAKEFRAG
$(stampfile):
	touch $@
endif

