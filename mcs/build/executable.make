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

ifndef PROGRAM_INSTALL_DIR
PROGRAM_INSTALL_DIR = $(prefix)/bin
endif

all-local: $(PROGRAM)

install-local: $(PROGRAM)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(PROGRAM_INSTALL_DIR)

uninstall-local:
	-rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog)

clean-local:
	-rm -f *.exe $(BUILT_SOURCES) $(CLEAN_FILES) $(pdb) $(stampfile) $(makefrag)
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -f $(response)
endif

ifndef HAS_TEST
test-local: $(PROGRAM)

run-test-local:

run-test-ondotnet-local:
endif

DISTFILES = $(sourcefile) $(EXTRA_DISTFILES)

dist-local: dist-default
	for f in `cat $(sourcefile)` ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

# Changing makefile probably means changing the
# sources, so let's be safe and add a Makefile dep

ifndef PROGRAM_COMPILE
PROGRAM_COMPILE = $(CSCOMPILE)
endif

$(PROGRAM): $(makefrag) $(response) $(stampfile)
	$(PROGRAM_COMPILE) /target:exe /out:$@ $(BUILT_SOURCES) @$(response)

# warning: embedded tab in the 'echo touch' line
$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): $(BUILT_SOURCES) \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
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

