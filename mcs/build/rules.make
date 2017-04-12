# -*- makefile -*-
#
# This is the makefile fragment with default rules
# for building things in MCS
#
# To customize the build, you should edit config.make.
# If you need to edit this file, that's a bug; email
# peter@newton.cx about it.

empty :=
space := $(empty) $(empty)

# given $(thisdir), we compute the path to the top directory
#
# split_path = $(filter-out .,$(subst /,$(space),$(1)))
# make_path = ./$(subst $(space),/,$(1))
# dotdottify = $(patsubst %,..,$(1))
# topdir = $(call make_path,$(call dotdottify,$(call split_path,$(thisdir))))
topdir := ./$(subst $(space),/,$(patsubst %,..,$(filter-out .,$(subst /,$(space),$(thisdir)))))

VERSION = 0.93

Q=$(if $(V),,@)
# echo -e "\\t" does not work on some systems, so use 5 spaces
Q_MCS=$(if $(V),,@echo "$(if $(MCS_MODE),MCS,CSC)     [$(intermediate)$(PROFILE)] $(notdir $(@))";)
Q_AOT=$(if $(V),,@echo "AOT     [$(intermediate)$(PROFILE)] $(notdir $(@))";)

ifndef BUILD_TOOLS_PROFILE
BUILD_TOOLS_PROFILE = build
endif

USE_MCS_FLAGS = /codepage:$(CODEPAGE) /nologo /noconfig /deterministic $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS) $(MCS_FLAGS)
USE_MBAS_FLAGS = /codepage:$(CODEPAGE) $(LOCAL_MBAS_FLAGS) $(PLATFORM_MBAS_FLAGS) $(PROFILE_MBAS_FLAGS) $(MBAS_FLAGS)
USE_CFLAGS = $(LOCAL_CFLAGS) $(CFLAGS) $(CPPFLAGS)
CSCOMPILE = $(Q_MCS) $(MCS) $(USE_MCS_FLAGS)
CSC_RUNTIME_FLAGS = --aot-path=$(abspath $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)) --gc-params=nursery-size=64m
BASCOMPILE = $(MBAS) $(USE_MBAS_FLAGS)
CCOMPILE = $(CC) $(USE_CFLAGS)
BOOT_COMPILE = $(Q_MCS) $(BOOTSTRAP_MCS) $(USE_MCS_FLAGS)
INSTALL = $(SHELL) $(topdir)/../mono/install-sh
INSTALL_DATA = $(INSTALL) -c -m 644
INSTALL_BIN = $(INSTALL) -c -m 755
INSTALL_LIB = $(INSTALL_BIN)
MKINSTALLDIRS = $(SHELL) $(topdir)/mkinstalldirs
INTERNAL_MBAS = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/mbas/mbas.exe
INTERNAL_ILASM = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/ilasm.exe
INTERNAL_CSC_LOCATION = $(CSC_LOCATION)

# Using CSC_SDK_PATH_DISABLED for sanity check that all references have path specified
INTERNAL_CSC = CSC_SDK_PATH_DISABLED= $(RUNTIME) $(RUNTIME_FLAGS) $(CSC_RUNTIME_FLAGS) $(INTERNAL_CSC_LOCATION)

RESGEN = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(RESGEN_EXE) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/resgen.exe
STRING_REPLACER = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/cil-stringreplacer.exe

depsdir = $(topdir)/build/deps

# Make sure these propagate if set manually

export PLATFORM
export PROFILE
export MCS
export MCS_FLAGS
export CC
export CFLAGS
export INSTALL
export MKINSTALLDIRS
export BOOTSTRAP_MCS
export DESTDIR
export RESGEN

# Get this so the platform.make platform-check rule doesn't become the
# default target

.DEFAULT: all
default: all

# Get initial configuration. pre-config is so that the builder can
# override PLATFORM or PROFILE

include $(topdir)/build/config-default.make
-include $(topdir)/build/pre-config.make
-include $(topdir)/build/config.make

# Default PLATFORM and PROFILE if they're not already defined.

ifndef PLATFORM
ifeq ($(OS),Windows_NT)
ifneq ($(V),)
$(info *** Assuming PLATFORM is 'win32'.)
endif
PLATFORM = win32
else
ifneq ($(V),)
$(info *** Assuming PLATFORM is 'linux'.)
endif
PLATFORM = linux
endif
endif

# Platform config

include $(topdir)/build/platforms/$(PLATFORM).make

ifdef PLATFORM_CORLIB
corlib = $(PLATFORM_CORLIB)
endif
# Useful

ifeq ($(PLATFORM_RUNTIME),$(RUNTIME))
PLATFORM_MONO_NATIVE = yes
endif

# Rest of the configuration

ifndef PROFILE
PROFILE = $(DEFAULT_PROFILE)
endif

include $(topdir)/build/profiles/$(PROFILE).make

ifdef BCL_OPTIMIZE
PROFILE_MCS_FLAGS += -optimize
endif

ifdef MCS_MODE
INTERNAL_CSC_LOCATION = $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/mcs.exe

ifdef PLATFORM_DEBUG_FLAGS
PLATFORM_DEBUG_FLAGS = /debug:full
endif

endif

# Design:
# Problem: We want to be able to build aot
# assemblies as part of the build system. 
#
# For this to be done safely, we really need two passes. This
# ensures that all of the .dlls are compiled before trying to
# aot them. Because we want this to be the
# default target for some profiles(testing_aot_full) we have a
# two-level build system. The do-all-aot target is what
# gets invoked at the top-level when someone tries to build with aot.
# It will invoke the do-all target, and will set TOP_LEVEL_DO for this
# recursive make call in order to prevent this recursive call from trying
# to build aot in each of the subdirs. After this is done, we will aot
# everything that our building produced by aoting everything in
# mcs/class/lib/$(PROFILE)/
ifndef TOP_LEVEL_DO

ifdef ALWAYS_AOT
TOP_LEVEL_DO = do-all-aot
else
TOP_LEVEL_DO = do-all
endif # ALWAYS_AOT

endif # !TOP_LEVEL_DO

ifdef OVERRIDE_TARGET_ALL
all: all.override
else
all: $(TOP_LEVEL_DO)
endif

ifdef NO_INSTALL
GACUTIL = :
else
gacutil = $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/gacutil.exe
GACUTIL = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(gacutil)
endif

STD_TARGETS = test run-test run-test-ondotnet clean install uninstall doc-update

$(STD_TARGETS): %: do-%

ifdef PLATFORM_AOT_SUFFIX

do-all-aot:
	$(MAKE) do-all TOP_LEVEL_DO=do-all
	$(MAKE) aot-all-profile

# When we recursively call $(MAKE) aot-all-profile
# we will have created this directory, and so will
# be able to evaluate the .dylibs to make
ifneq ("$(wildcard $(topdir)/class/lib/$(PROFILE))","")

AOT_PROFILE_ASSEMBLIES := $(sort $(patsubst .//%,%,$(filter-out %.dll.dll %.exe.dll %bare% %plaincore% %secxml% %Facades% %ilasm%,$(filter %.dll %.exe,$(wildcard $(topdir)/class/lib/$(PROFILE)/*)))))

# This can run in parallel
.PHONY: aot-all-profile
ifdef AOT_BUILD_FLAGS
aot-all-profile: $(patsubst %,%$(PLATFORM_AOT_SUFFIX),$(AOT_PROFILE_ASSEMBLIES))
else
aot-all-profile:
	echo AOT_BUILD_FLAGS not set, skipping AOT.
endif

%.dll$(PLATFORM_AOT_SUFFIX): %.dll
	@ mkdir -p $<_bitcode_tmp
	$(Q_AOT) MONO_PATH="$(dir $<)" $(RUNTIME) $(RUNTIME_FLAGS) $(AOT_BUILD_FLAGS),temp-path=$<_bitcode_tmp --verbose $< > $@.aot-log
	@ rm -rf $<_bitcode_tmp

%.exe$(PLATFORM_AOT_SUFFIX): %.exe
	@ mkdir -p $<_bitcode_tmp
	$(Q_AOT) MONO_PATH="$(dir $<)" $(RUNTIME) $(RUNTIME_FLAGS) $(AOT_BUILD_FLAGS),temp-path=$<_bitcode_tmp --verbose $< > $@.aot-log
	@ rm -rf $<_bitcode_tmp

endif #ifneq ("$(wildcard $(topdir)/class/lib/$(PROFILE))","")

endif # PLATFORM_AOT_SUFFIX

do-run-test:
	ok=:; $(MAKE) run-test-recursive || ok=false; $(MAKE) run-test-local || ok=false; $$ok

do-%: %-recursive
	$(MAKE) $*-local

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):

csproj: do-csproj

# The way this is set up, any profile-specific subdirs list should
# be listed _before_ including rules.make.  However, the default
# SUBDIRS list can come after, so don't use the eager := syntax when
# using the defaults.
PROFILE_SUBDIRS := $($(PROFILE)_SUBDIRS)
ifndef PROFILE_SUBDIRS
PROFILE_SUBDIRS = $(SUBDIRS)
endif

# These subdirs can be built in parallel
PROFILE_PARALLEL_SUBDIRS := $($(PROFILE)_PARALLEL_SUBDIRS)
ifndef PROFILE_PARALLEL_SUBDIRS
PROFILE_PARALLEL_SUBDIRS = $(PARALLEL_SUBDIRS)
endif

ifndef FRAMEWORK_VERSION_MAJOR
FRAMEWORK_VERSION_MAJOR = $(basename $(FRAMEWORK_VERSION))
endif

%-recursive:
	@set . $$MAKEFLAGS; final_exit=:; \
	case $$2 in --unix) shift ;; esac; \
	case $$2 in *=*) dk="exit 1" ;; *k*) dk=: ;; *) dk="exit 1" ;; esac; \
	list='$(PROFILE_SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) $*) || { final_exit="exit 1"; $$dk; } ; \
	done; \
	if [ $* = "all" -a -n "$(PROFILE_PARALLEL_SUBDIRS)" ]; then \
		$(MAKE) do-all-parallel ENABLE_PARALLEL_SUBDIR_BUILD=1 || { final_exit="exit 1"; $$dk; } ; \
	else \
		list='$(PROFILE_PARALLEL_SUBDIRS)'; for d in $$list ; do \
		    (cd $$d && $(MAKE) $*) || { final_exit="exit 1"; $$dk; } ; \
		done; \
	fi; \
	$$final_exit

#
# Parallel build support
#
# The variable $(PROFILE)_PARALLEL_SUBDIRS should be set to the list of directories
# which could be built in parallel. These directories are built after the directories in
# $(PROFILE)_SUBDIRS.
# Parallel building is currently only supported for the 'all' target.
#
# Each directory's Makefile may define DEP_LIBS and DEP_DIRS to specify the libraries and
# directories it depends on.
#
ifneq ($(PROFILE_PARALLEL_SUBDIRS),)
dep_dirs = .dep_dirs-$(PROFILE)
$(dep_dirs):
	@echo "Creating $@..."
	list='$(PROFILE_PARALLEL_SUBDIRS)'; \
	echo > $@; \
	for d in $$list; do \
		$(MAKE) -C $$d gen-deps DEPS_TARGET_DIR=$$d DEPS_FILE=$(abspath $@); \
	done
-include $(dep_dirs)
endif

.PHONY: gen-deps
# The gen-deps target is in library.make/executable.make so it can pick up
# DEP_LIBS/DEP_DIRS

clean-dep-dir:
	$(RM) $(dep_dirs)

clean-local: clean-dep-dir

ifdef ENABLE_PARALLEL_SUBDIR_BUILD
.PHONY: do-all-parallel $(PROFILE_PARALLEL_SUBDIRS)

do-all-parallel: $(PROFILE_PARALLEL_SUBDIRS)

$(PROFILE_PARALLEL_SUBDIRS):
	@set . $$MAKEFLAGS; \
	cd $@ && $(MAKE)
endif

ifndef DIST_SUBDIRS
DIST_SUBDIRS = $(SUBDIRS) $(DIST_ONLY_SUBDIRS)
endif
dist-recursive: dist-local
	@case '$(distdir)' in [\\/$$]* | ?:[\\/]* ) reldir='$(distdir)' ;; *) reldir='../$(distdir)' ;; esac ; \
	list='$(DIST_SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) distdir=$$reldir/$$d $@) || exit 1 ; \
	done

# The following target can be used like
#
#   dist-local: dist-default
#	... additional commands ...
#
# Notes:
#  1. we invert the test here to not end in an error if ChangeLog doesn't exist.
#  2. we error out if we try to dist a nonexistant file.
#  3. we pick up Makefile, makefile, or GNUmakefile.
dist-default:
	-mkdir -p $(distdir)
	test '!' -f ChangeLog || cp ChangeLog $(distdir)
	if test -f Makefile; then m=M; fi; \
	if test -f makefile; then m=m; fi; \
	if test -f GNUmakefile; then m=GNUm; fi; \
	for f in $${m}akefile $(DISTFILES) ; do \
	    dest=`dirname "$(distdir)/$$f"` ; \
	    $(MKINSTALLDIRS) $$dest && cp -p "$$f" $$dest || exit 1 ; \
	done
	if test -d Documentation ; then \
		find . -name '*.xml' > .files ; \
		tar cTf .files - | (cd $(distdir); tar xf -) ; \
		rm .files ; \
	fi

%/.stamp:
	$(MKINSTALLDIRS) $(@D)
	touch $@

## Documentation stuff

Q_MDOC =$(if $(V),,@echo "MDOC    [$(PROFILE)] $(notdir $(@))";)
MDOC   =$(Q_MDOC) MONO_PATH="$(topdir)/class/lib/$(DEFAULT_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/$(DEFAULT_PROFILE)/mdoc.exe

