# -*- makefile -*-
#
# The rules for building a program.

base_prog = $(notdir $(PROGRAM))
prog_dir := $(filter-out . ./, $(dir $(PROGRAM)))
ifndef sourcefile
sourcefile := $(base_prog).sources
endif
base_prog_config := $(wildcard $(base_prog).config.$(PROFILE))
ifndef base_prog_config
base_prog_config := $(wildcard $(base_prog).config)
endif
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

ifdef KEEP_OUTPUT_FILE_COPY
	COPY_CMD = cp
else
	COPY_CMD = mv
endif

makefrag = $(depsdir)/$(PROFILE)_$(base_prog).makefrag
pdb = $(patsubst %.exe,%.pdb,$(PROGRAM))
mdb = $(patsubst %.exe,%.mdb,$(PROGRAM))
executable_CLEAN_FILES += $(makefrag) $(pdb) $(mdb)

all-local: $(PROGRAM) $(PROGRAM_config)

install-local: all-local
test-local: all-local
uninstall-local:

ifdef NO_INSTALL
install-local uninstall-local:
	@:
else

ifndef PROGRAM_INSTALL_DIR
PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
endif

install-local: $(PROGRAM) $(PROGRAM_config)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	test ! -f $(PROGRAM).mdb || $(INSTALL_BIN) $(PROGRAM).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)
ifdef PROGRAM_config
	$(INSTALL_DATA) $(PROGRAM_config) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
endif
ifdef PLATFORM_AOT_SUFFIX
	test ! -f $(PROGRAM)$(PLATFORM_AOT_SUFFIX) || $(INSTALL_LIB) $(PROGRAM)$(PLATFORM_AOT_SUFFIX) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
endif

uninstall-local:
	-rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog) $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).config
endif

clean-local:
	-rm -f $(executable_CLEAN_FILES) $(CLEAN_FILES) $(tests_CLEAN_FILES)

test-local:
	@:
run-test-local:
	@:
run-test-ondotnet-local:
	@:

DISTFILES = $(sourcefile) $(base_prog_config) $(EXTRA_DISTFILES)

ifdef HAS_NUNIT_TEST
ASSEMBLY      = $(PROGRAM)
ASSEMBLY_EXT  = .exe
the_assembly  = $(PROGRAM)
include $(topdir)/build/tests.make
endif

ifdef HAVE_CS_TESTS
DISTFILES += $(test_sourcefile)
endif

dist-local: dist-default
	for f in `cat $(sourcefile)` ; do \
	  case $$f in \
	  ../*) : ;; \
	  *) dest=`dirname "$$f"` ; \
	     case $$subs in *" $$dest "*) : ;; *) subs=" $$dest$$subs" ; $(MKINSTALLDIRS) $(distdir)/$$dest ;; esac ; \
	     cp -p "$$f" $(distdir)/$$dest || exit 1 ;; \
	  esac ; done ; \
	for d in . $$subs ; do \
	  case $$d in .) : ;; *) test ! -f $$d/ChangeLog || cp -p $$d/ChangeLog $(distdir)/$$d ;; esac ; done

ifndef PROGRAM_COMPILE
PROGRAM_COMPILE = $(CSCOMPILE)
endif

$(PROGRAM): $(BUILT_SOURCES) $(EXTRA_SOURCES) $(response) $(prog_dir:=/.stamp)
	$(PROGRAM_COMPILE) -target:exe -out:$(base_prog) $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(response)
ifneq ($(base_prog),$(PROGRAM))
	$(COPY_CMD) $(base_prog) $(PROGRAM)
	test ! -f $(base_prog).mdb || $(COPY_CMD) $(base_prog).mdb $(PROGRAM).mdb
endif

ifdef PROGRAM_config
ifneq ($(base_prog_config),$(PROGRAM_config))
executable_CLEAN_FILES += $(PROGRAM_config)
$(PROGRAM_config): $(base_prog_config) $(dir $(PROGRAM_config))/.stamp
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

all-local: $(makefrag) $(extra_targets)

csproj-local:
	config_file=`basename $(PROGRAM) .exe`-$(PROFILE).input; \
	echo $(thisdir):$$config_file >> $(topdir)/../msvc/scripts/order; \
	(echo $(is_boot); \
	echo $(MCS);	\
	echo $(USE_MCS_FLAGS) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS); \
	echo $(PROGRAM); \
	echo $(BUILT_SOURCES_cmdline); \
	echo $(build_lib); \
	echo $(response)) > $(topdir)/../msvc/scripts/inputs/$$config_file


ifneq ($(response),$(sourcefile))
$(response): $(topdir)/build/executable.make $(depsdir)/.stamp
endif
$(makefrag): $(topdir)/build/executable.make $(depsdir)/.stamp

doc-update-local:
	@:

