# -*- makefile -*-
#
# The rules for building a program.

base_prog = $(notdir $(PROGRAM))
ifndef sourcefile
sourcefile := $(base_prog).sources
endif
base_prog_config := $(wildcard $(base_prog).config.$(PROFILE))
ifndef base_prog_config
base_prog_config := $(wildcard $(base_prog).config)
endif

ifeq (cat,$(PLATFORM_CHANGE_SEPARATOR_CMD))
response = $(sourcefile)
else
response = $(depsdir)/$(sourcefile).response
executable_CLEAN_FILES += $(response)
endif

ifndef the_libdir
the_libdir = $(topdir)/class/lib/$(PROFILE)/
ifdef PROGRAM_USE_INTERMEDIATE_FILE
build_libdir = $(the_libdir)tmp/
else
build_libdir = $(the_libdir)
endif
endif

ifdef base_prog_config
PROGRAM_config := $(build_libdir)$(PROGRAM).config
endif

the_lib = $(the_libdir)$(base_prog)
build_lib = $(build_libdir)$(base_prog)

executable_CLEAN_FILES += $(the_lib)   $(the_lib).so   $(the_lib).mdb   $(the_lib:.exe=.pdb)
executable_CLEAN_FILES += $(build_lib) $(build_lib).so $(build_lib).mdb $(build_lib:.exe=.pdb)

makefrag = $(depsdir)/$(PROFILE)_$(base_prog).makefrag

all-local: $(the_lib) $(PROGRAM_config)

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

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) $(the_lib) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	test ! -f $(the_lib).mdb || $(INSTALL_BIN) $(the_lib).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)
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

$(the_lib): $(the_libdir)/.stamp

$(build_lib): $(BUILT_SOURCES) $(EXTRA_SOURCES) $(response) $(build_libdir:=/.stamp)
	$(PROGRAM_COMPILE) -target:exe -out:$@ $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(response)

ifdef PROGRAM_USE_INTERMEDIATE_FILE
$(the_lib): $(build_lib)
	$(Q) cp $(build_lib) $@
	$(Q) test ! -f $(build_lib).mdb || mv $(build_lib).mdb $@.mdb
	$(Q) test ! -f $(build_lib:.exe=.pdb) || mv $(build_lib:.exe=.pdb) $(the_lib:.exe=.pdb)
endif

ifdef PROGRAM_config
ifneq ($(base_prog_config),$(PROGRAM_config))
executable_CLEAN_FILES += $(PROGRAM_config)
$(PROGRAM_config): $(base_prog_config) $(dir $(PROGRAM_config))/.stamp
	cp $(base_prog_config) $(PROGRAM_config)
endif
endif

$(makefrag): $(sourcefile)
#	@echo Creating $@ ...
	@sed 's,^,$(build_lib): ,' $< >$@
	@if test ! -f $(sourcefile).makefrag; then :; else \
	   cat $(sourcefile).makefrag >> $@ ; \
	   echo '$@: $(sourcefile).makefrag' >> $@; \
	   echo '$(sourcefile).makefrag:' >> $@; fi

ifneq ($(response),$(sourcefile))
$(response): $(sourcefile)
	@echo Converting $(sourcefile) to $@ ...
	@cat $(sourcefile) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

all-local: $(makefrag) $(extra_targets)

csproj-local:
	config_file=`basename $(PROGRAM) .exe`-$(PROFILE).input; \
	echo $(thisdir):$$config_file >> $(topdir)/../msvc/scripts/order; \
	(echo $(is_boot); \
	echo $(USE_MCS_FLAGS) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS); \
	echo $(PROGRAM); \
	echo $(BUILT_SOURCES_cmdline); \
	echo $(build_lib); \
	echo $(FRAMEWORK_VERSION); \
	echo $(PROFILE); \
	echo $(response)) > $(topdir)/../msvc/scripts/inputs/$$config_file


ifneq ($(response),$(sourcefile))
$(response): $(topdir)/build/executable.make $(depsdir)/.stamp
endif
$(makefrag): $(topdir)/build/executable.make $(depsdir)/.stamp

doc-update-local:
	@:

