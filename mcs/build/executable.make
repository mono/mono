# -*- makefile -*-
#
# The rules for building a program.

# I'd rather not create a response file here,
# but since on Win32 we need to munge the paths
# anyway, we might as well.

base_prog = $(shell basename $(PROGRAM))
sourcefile = $(base_prog).sources
PROGRAM_config := $(wildcard $(PROGRAM).config)

ifeq (cat,$(PLATFORM_CHANGE_SEPARATOR_CMD))
response = $(sourcefile)
else
response = $(depsdir)/$(base_prog).response
executable_CLEAN_FILES += $(response)
endif

makefrag = $(depsdir)/$(base_prog).makefrag
pdb = $(patsubst %.exe,%.pdb,$(PROGRAM))
mdb = $(patsubst %.exe,%.mdb,$(PROGRAM))
executable_CLEAN_FILES += $(makefrag) $(pdb) $(mdb)

ifndef PROGRAM_INSTALL_DIR
PROGRAM_INSTALL_DIR = $(prefix)/bin
endif

all-local: $(PROGRAM)

install-local: $(PROGRAM) $(PROGRAM_config)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	-$(INSTALL_BIN) $(PROGRAM).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)
ifdef PROGRAM_config
	$(INSTALL_DATA) $(PROGRAM_config) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
endif

uninstall-local:
	-rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog) $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).mdb

clean-local:
	-rm -f *.exe $(BUILT_SOURCES) $(executable_CLEAN_FILES) $(CLEAN_FILES)

test-local: $(PROGRAM)
	@:
run-test-local:
	@:
run-test-ondotnet-local:
	@:

DISTFILES = $(sourcefile) $(EXTRA_DISTFILES)

dist-local: dist-default
	for f in `cat $(sourcefile)` ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

ifndef PROGRAM_COMPILE
PROGRAM_COMPILE = $(CSCOMPILE)
endif

$(PROGRAM): $(BUILT_SOURCES) $(EXTRA_SOURCES) $(response)
	$(PROGRAM_COMPILE) /target:exe /out:$@ $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(response)

$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@sed 's,^,$(PROGRAM): ,' $< > $@

ifneq ($(response),$(sourcefile))
$(response): $(sourcefile)
	@echo Creating $@ ...
	@( $(PLATFORM_CHANGE_SEPARATOR_CMD) ) <$< >$@
endif

-include $(makefrag)

all-local: $(makefrag)
$(makefrag): $(topdir)/build/executable.make
