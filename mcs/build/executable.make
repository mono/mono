# -*- makefile -*-
#
# The rules for building a program.

base_prog = $(shell basename $(PROGRAM))
sourcefile = $(base_prog).sources
base_prog_config := $(wildcard $(base_prog).config)
ifdef base_prog_config
PROGRAM_config := $(PROGRAM).config
endif

executable_CLEAN_FILES = *.exe $(PROGRAM) $(PROGRAM).mdb $(BUILT_SOURCES)

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

all-local: $(PROGRAM) $(PROGRAM_config)

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
	-rm -f $(executable_CLEAN_FILES) $(CLEAN_FILES)

test-local: $(PROGRAM)
	@:
run-test-local:
	@:
run-test-ondotnet-local:
	@:

DISTFILES = $(sourcefile) $(base_prog_config) $(EXTRA_DISTFILES)

dist-local: dist-default
	for f in `cat $(sourcefile)` ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

ifndef PROGRAM_COMPILE
PROGRAM_COMPILE = $(CSCOMPILE)
endif

$(PROGRAM): $(BUILT_SOURCES) $(EXTRA_SOURCES) $(response)
	$(PROGRAM_COMPILE) /target:exe /out:$(base_prog) $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(response)
ifneq ($(base_prog),$(PROGRAM))
	mv $(base_prog) $(PROGRAM)
	-mv $(base_prog).mdb $(PROGRAM).mdb
endif

ifdef PROGRAM_config
ifneq ($(base_prog_config),$(PROGRAM_config))
executable_CLEAN_FILES += $(PROGRAM_config)
$(PROGRAM_config): $(base_prog_config)
	cp $(base_prog_config) $(PROGRAM_config)
endif
endif

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
