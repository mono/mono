# -*- makefile -*-
#
# The rules for building our class libraries.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

# All the dep files now land in the same directory so we
# munge in the library name to keep the files from clashing.

# The including makefile can set the following variables:
# LIB_MCS_FLAGS - Command line flags passed to mcs.
# LIB_REFS      - This should be a space separated list of assembly names which are added to the mcs
#                 command line.
#

# All dependent libs become dependent dirs for parallel builds
# Have to rename to handle differences between assembly/directory names
DEP_LIBS=$(patsubst System.Xml,System.XML,$(LIB_REFS))

_FILTER_OUT = $(foreach x,$(2),$(if $(findstring $(1),$(x)),,$(x)))

LIB_REFS_FULL = $(call _FILTER_OUT,=, $(LIB_REFS))
LIB_REFS_ALIAS = $(filter-out $(LIB_REFS_FULL),$(LIB_REFS))

LIB_MCS_FLAGS += $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/%.dll,$(LIB_REFS_FULL))
LIB_MCS_FLAGS += $(patsubst %,-r:%.dll, $(subst =,=$(topdir)/class/lib/$(PROFILE)/,$(LIB_REFS_ALIAS)))

sourcefile = $(LIBRARY).sources

# If the directory contains the per profile include file, generate list file.
PROFILE_sources := $(wildcard $(PROFILE)_$(LIBRARY).sources)
ifdef PROFILE_sources
PROFILE_excludes = $(wildcard $(PROFILE)_$(LIBRARY).exclude.sources)
sourcefile = $(depsdir)/$(PROFILE)_$(LIBRARY).sources
library_CLEAN_FILES += $(sourcefile)

ifdef EXTENSION_MODULE
EXTENSION_include = $(wildcard $(topdir)/../../mono-extensions/mcs/$(thisdir)/$(PROFILE)_$(LIBRARY).sources)
else
EXTENSION_include = $(wildcard $(PROFILE)_opt_$(LIBRARY).sources)
endif


ifdef EXTENSION_MODULE
EXTENSION_exclude = $(wildcard $(topdir)/../../mono-extensions/mcs/$(thisdir)/$(PROFILE)_$(LIBRARY).exclude.sources)
else
EXTENSION_exclude = $(wildcard $(PROFILE)_opt_$(LIBRARY).exclude.sources)
endif

# Note, gensources.sh can create a $(sourcefile).makefrag if it sees any '#include's
# We don't include it in the dependencies since it isn't always created
$(sourcefile): $(PROFILE_sources) $(PROFILE_excludes) $(topdir)/build/gensources.sh $(EXTENSION_include)
	@echo Creating the per profile list $@ ...
	$(SHELL) $(topdir)/build/gensources.sh $@ '$(PROFILE_sources)' '$(PROFILE_excludes)' '$(EXTENSION_include)' '$(EXTENSION_exclude)'
endif

PLATFORM_excludes := $(wildcard $(LIBRARY).$(PLATFORM)-excludes)

ifndef PLATFORM_excludes
ifeq (cat,$(PLATFORM_CHANGE_SEPARATOR_CMD))
response = $(sourcefile)
endif
endif

ifndef response
response = $(depsdir)/$(PROFILE)_$(LIBRARY_SUBDIR)_$(LIBRARY).response
library_CLEAN_FILES += $(response)
endif

ifndef LIBRARY_NAME
LIBRARY_NAME = $(LIBRARY)
endif

ifdef LIBRARY_COMPAT
lib_dir = compat
else
lib_dir = lib
endif

ifdef LIBRARY_SUBDIR
the_libdir_base = $(topdir)/class/$(lib_dir)/$(PROFILE)/$(LIBRARY_SUBDIR)/
else
the_libdir_base = $(topdir)/class/$(lib_dir)/$(PROFILE)/
endif

ifdef RESOURCE_STRINGS
ifdef BOOTSTRAP_PROFILE
ifneq (basic, $(BUILD_TOOLS_PROFILE))
RESOURCE_STRINGS_FILES += $(RESOURCE_STRINGS:%=--resourcestrings:%)
endif
endif
endif

#
# The bare directory contains the plain versions of System and System.Xml
#
bare_libdir = $(the_libdir_base)bare

#
# The secxml directory contains the System version that depends on 
# System.Xml and Mono.Security
#
secxml_libdir = $(the_libdir_base)secxml

the_libdir = $(the_libdir_base)$(intermediate)

ifdef LIBRARY_USE_INTERMEDIATE_FILE
build_libdir = $(the_libdir)tmp/
else
build_libdir = $(the_libdir)
endif

the_lib   = $(the_libdir)$(LIBRARY_NAME)
build_lib = $(build_libdir)$(LIBRARY_NAME)
library_CLEAN_FILES += $(the_lib)   $(the_lib).so   $(the_lib).mdb   $(the_lib:.dll=.pdb)
library_CLEAN_FILES += $(build_lib) $(build_lib).so $(build_lib).mdb $(build_lib:.dll=.pdb)

ifdef NO_SIGN_ASSEMBLY
SN = :
else
ifeq ("$(SN)","")
sn = $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/sn.exe
SN = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(sn) -q
endif
endif

ifeq ($(PLATFORM), win32)
GACDIR = `cygpath -w $(mono_libdir)`
GACROOT = `cygpath -w $(DESTDIR)$(mono_libdir)`
test_flags += -d:WINDOWS
else
GACDIR = $(mono_libdir)
GACROOT = $(DESTDIR)$(mono_libdir)
endif

ifndef NO_BUILD
all-local: $(the_lib) $(extra_targets)
endif

ifeq ($(LIBRARY_COMPILE),$(BOOT_COMPILE))
is_boot=true
else
is_boot=false
endif

csproj-local: csproj-library csproj-test

intermediate_clean=$(subst /,-,$(intermediate))
csproj-library: 
	config_file=`basename $(LIBRARY) .dll`-$(intermediate_clean)$(PROFILE).input; \
	case "$(thisdir)" in *"Facades"*) config_file=Facades_$$config_file;; esac; \
	echo $(thisdir):$$config_file >> $(topdir)/../msvc/scripts/order; \
	(echo $(is_boot); \
	echo $(USE_MCS_FLAGS) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS); \
	echo $(LIBRARY_NAME); \
	echo $(BUILT_SOURCES_cmdline); \
	echo $(build_lib); \
	echo $(FRAMEWORK_VERSION); \
	echo $(PROFILE); \
	echo $(response)) > $(topdir)/../msvc/scripts/inputs/$$config_file

csproj-test:

install-local: all-local
test-local: all-local
uninstall-local:

ifdef NO_INSTALL
install-local uninstall-local:
	@:

else

aot_lib = $(the_lib)$(PLATFORM_AOT_SUFFIX)
aot_libname = $(LIBRARY_NAME)$(PLATFORM_AOT_SUFFIX)

ifdef LIBRARY_INSTALL_DIR
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME)
	test ! -f $(the_lib).mdb || $(INSTALL_LIB) $(the_lib).mdb $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME).mdb
ifdef PLATFORM_AOT_SUFFIX
	test ! -f $(aot_lib) || $(INSTALL_LIB) $(aot_lib) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
endif

uninstall-local:
	-rm -f $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME).mdb

else

# If RUNTIME_HAS_CONSISTENT_GACDIR is set, it implies that the internal GACDIR
# of the runtime is the same as the GACDIR we want.  So, we don't need to pass it
# to gacutil.  Note that the GACDIR we want may not be the same as the value of
# GACDIR set above, since the user could have overridden the value of $(prefix).
#
# This makes a difference only when we're building from the mono/ tree, since we
# have to ensure that the internal GACDIR of the in-tree runtime matches where we
# install the DLLs.

ifndef RUNTIME_HAS_CONSISTENT_GACDIR
gacdir_flag = /gacdir $(GACDIR)
endif

ifndef LIBRARY_PACKAGE
ifdef LIBRARY_COMPAT
LIBRARY_PACKAGE = compat-$(FRAMEWORK_VERSION)
else
LIBRARY_PACKAGE = $(FRAMEWORK_VERSION)
endif
endif

ifneq (none, $(LIBRARY_PACKAGE))
package_flag = /package $(LIBRARY_PACKAGE)
endif

install-local: $(gacutil)
	$(GACUTIL) /i $(the_lib) /f $(gacdir_flag) /root $(GACROOT) $(package_flag)

uninstall-local: $(gacutil)
	-$(GACUTIL) /u $(LIBRARY_NAME:.dll=) $(gacdir_flag) /root $(GACROOT) $(package_flag)

endif # LIBRARY_INSTALL_DIR
endif # NO_INSTALL

clean-local:
	-rm -f $(tests_CLEAN_FILES) $(library_CLEAN_FILES) $(CLEAN_FILES)

test-local run-test-local run-test-ondotnet-local:
	@:

DISTFILES = $(wildcard *$(LIBRARY)*.sources) $(EXTRA_DISTFILES)

ASSEMBLY      = $(LIBRARY)
ASSEMBLY_EXT  = .dll
the_assembly  = $(the_lib)
include $(topdir)/build/tests.make

ifdef HAVE_CS_TESTS
DISTFILES += $(test_sourcefile)

csproj-test:
	config_file=`basename $(LIBRARY) .dll`-tests-$(PROFILE).input; \
	echo $(thisdir):$$config_file >> $(topdir)/../msvc/scripts/order; \
	(echo false; \
	echo $(USE_MCS_FLAGS) -r:$(the_assembly) $(TEST_MCS_FLAGS); \
	echo $(test_lib); \
	echo $(BUILT_SOURCES_cmdline); \
	echo $(test_lib); \
	echo $(FRAMEWORK_VERSION); \
	echo $(PROFILE); \
	echo $(test_response)) > $(topdir)/../msvc/scripts/inputs/$$config_file

endif

# make dist will collect files in .sources files from all profiles
dist-local: dist-default
	subs=' ' ; \
	for f in `$(topdir)/tools/removecomments.sh $(wildcard *$(LIBRARY).sources)` $(TEST_FILES) ; do \
	  case $$f in \
	  ../*) : ;; \
	  *.g.cs) : ;; \
	  *) dest=`dirname "$$f"` ; \
	     case $$subs in *" $$dest "*) : ;; *) subs=" $$dest$$subs" ; $(MKINSTALLDIRS) $(distdir)/$$dest ;; esac ; \
	     cp -p "$$f" $(distdir)/$$dest || exit 1 ;; \
	  esac ; done ; \
	for d in . $$subs ; do \
	  case $$d in .) : ;; *) test ! -f $$d/ChangeLog || cp -p $$d/ChangeLog $(distdir)/$$d ;; esac ; done

ifndef LIBRARY_COMPILE
LIBRARY_COMPILE = $(CSCOMPILE)
endif

ifndef LIBRARY_SNK
LIBRARY_SNK = $(topdir)/class/mono.snk
endif

ifdef BUILT_SOURCES
library_CLEAN_FILES += $(BUILT_SOURCES)
ifeq (cat, $(PLATFORM_CHANGE_SEPARATOR_CMD))
BUILT_SOURCES_cmdline = $(BUILT_SOURCES)
else
BUILT_SOURCES_cmdline = `echo $(BUILT_SOURCES) | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
endif
endif

# The library

$(the_lib): $(the_libdir)/.stamp

$(build_lib): $(response) $(sn) $(BUILT_SOURCES) $(build_libdir:=/.stamp)
	$(LIBRARY_COMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) -target:library -out:$@ $(BUILT_SOURCES_cmdline) @$(response)
	$(Q) $(SN) -R $@ $(LIBRARY_SNK)
ifdef RESOURCE_STRINGS_FILES
	$(Q) $(STRING_REPLACER) $(RESOURCE_STRINGS_FILES) $@
endif

ifdef LIBRARY_USE_INTERMEDIATE_FILE
$(the_lib): $(build_lib)
	$(Q) cp $(build_lib) $@
	$(Q) $(SN) -v $@
	$(Q) test ! -f $(build_lib).mdb || mv $(build_lib).mdb $@.mdb
	$(Q) test ! -f $(build_lib:.dll=.pdb) || mv $(build_lib:.dll=.pdb) $(the_lib:.dll=.pdb)
endif

library_CLEAN_FILES += $(PROFILE)_aot.log

ifdef PLATFORM_AOT_SUFFIX
Q_AOT=$(if $(V),,@echo "AOT     [$(PROFILE)] $(notdir $(@))";)

$(the_lib)$(PLATFORM_AOT_SUFFIX): $(the_lib)
	$(Q_AOT) MONO_PATH='$(the_libdir_base)' > $(PROFILE)_$(LIBRARY_NAME)_aot.log 2>&1 $(RUNTIME) $(AOT_BUILD_FLAGS) --debug $(the_lib)

all-local-aot: $(the_lib)$(PLATFORM_AOT_SUFFIX)
endif


makefrag = $(depsdir)/$(PROFILE)_$(LIBRARY_SUBDIR)_$(LIBRARY).makefrag
library_CLEAN_FILES += $(makefrag)
$(makefrag): $(sourcefile)
#	@echo Creating $@ ...
	@sed 's,^,$(build_lib): ,' $< >$@
	@if test ! -f $(sourcefile).makefrag; then :; else \
	   cat $(sourcefile).makefrag >> $@ ; \
	   echo '$@: $(sourcefile).makefrag' >> $@; \
	   echo '$(sourcefile).makefrag:' >> $@; fi

ifneq ($(response),$(sourcefile))

ifdef PLATFORM_excludes
$(response): $(sourcefile) $(PLATFORM_excludes)
	@echo Filtering $(sourcefile) to $@ ...
	@sort $(sourcefile) $(PLATFORM_excludes) | uniq -u | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
$(response): $(sourcefile)
	@echo Converting $(sourcefile) to $@ ...
	@cat $(sourcefile) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

endif

-include $(makefrag)

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

## Include corcompare stuff
include $(topdir)/build/corcompare.make

all-local: $(makefrag) $(test_makefrag) $(btest_makefrag)
ifneq ($(response),$(sourcefile))
$(response): $(topdir)/build/library.make $(depsdir)/.stamp
endif
$(makefrag) $(test_response) $(test_makefrag) $(btest_response) $(btest_makefrag): $(topdir)/build/library.make $(depsdir)/.stamp

## Documentation stuff

Q_MDOC_UP=$(if $(V),,@echo "MDOC-UP [$(PROFILE)] $(notdir $(@))";)
MDOC_UP  =$(Q_MDOC_UP) \
		MONO_PATH="$(topdir)/class/lib/$(DEFAULT_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/$(DEFAULT_PROFILE)/mdoc.exe \
		update --delete -o Documentation/en $(the_lib)

doc-update-local: $(the_libdir)/.doc-stamp

$(the_libdir)/.doc-stamp: $(the_lib)
	$(MDOC_UP)
	@echo "doc-stamp" > $@

# Need to be here so it comes after the definition of DEP_DIRS/DEP_LIBS
gen-deps:
	@echo "$(DEPS_TARGET_DIR): $(DEP_DIRS) $(DEP_LIBS)" >> $(DEPS_FILE)
