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
comma := ,
_FILTER_OUT = $(foreach x,$(2),$(if $(findstring $(1),$(x)),,$(x)))

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
Q_MCS=$(if $(V),,@echo "$(if $(MCS_MODE),MCS,CSC)     [$(intermediate)$(PROFILE_DIRECTORY)] $(notdir $(@))";)
Q_AOT=$(if $(V),,@echo "AOT     [$(intermediate)$(PROFILE_DIRECTORY)] $(notdir $(@))";)

ifndef BUILD_TOOLS_PROFILE
BUILD_TOOLS_PROFILE = build
endif

# NOTE: We have to use conditional functions to branch on the state of ENABLE_COMPILER_SERVER
#  because the value of this flag can change after rules.make is evaluated. If we use regular ifeq
#  statements our builds will opt to use or not use the compiler server seemingly at random because
#  we include rules.make many times and the last observed value from rules.make does not match the
#  value used when actually executing csc. you can observe this by adding an $ ( info here and an
#  echo above the csc invocation and printing the values.
COMPILER_SERVER_ENABLED_ARGS = /shared:$(COMPILER_SERVER_PIPENAME)
COMPILER_SERVER_ARGS = $(if $(findstring 1,$(ENABLE_COMPILER_SERVER)),$(COMPILER_SERVER_ENABLED_ARGS),)
CSC_LOCATION = $(if $(findstring 1,$(ENABLE_COMPILER_SERVER)),$(SERVER_CSC_LOCATION),$(STANDALONE_CSC_LOCATION))

USE_MCS_FLAGS = $(COMPILER_SERVER_ARGS) /codepage:$(CODEPAGE) /nologo /noconfig /deterministic $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS) $(MCS_FLAGS)
USE_CFLAGS = $(LOCAL_CFLAGS) $(CFLAGS) $(CPPFLAGS)
CSCOMPILE = $(Q_MCS) $(MCS) $(USE_MCS_FLAGS)
CSC_RUNTIME_FLAGS = --aot-path=$(abspath $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)) --gc-params=nursery-size=64m
CCOMPILE = $(CC) $(USE_CFLAGS)
BOOT_COMPILE = $(Q_MCS) $(BOOTSTRAP_MCS) $(USE_MCS_FLAGS)
INSTALL = $(SHELL) $(topdir)/../mono/install-sh
INSTALL_DATA = $(INSTALL) -c -m 644
INSTALL_BIN = $(INSTALL) -c -m 755
INSTALL_LIB = $(INSTALL_BIN)
MKINSTALLDIRS = $(SHELL) $(topdir)/mkinstalldirs
INTERNAL_CSC_LOCATION = $(CSC_LOCATION)

# Using CSC_SDK_PATH_DISABLED for sanity check that all references have path specified
INTERNAL_CSC = CSC_SDK_PATH_DISABLED= $(RUNTIME) $(RUNTIME_FLAGS) $(CSC_RUNTIME_FLAGS) $(INTERNAL_CSC_LOCATION)

RESGEN = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/resgen.exe
STRING_REPLACER = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/cil-stringreplacer.exe
ILASM = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/ilasm.exe
GENSOURCES = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/gensources.exe

depsdir = $(topdir)/build/deps

# Make sure these propagate if set manually

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
# override BUILD_PLATFORM or PROFILE

include $(topdir)/build/config-default.make
-include $(topdir)/build/pre-config.make
-include $(topdir)/build/config.make

# Platform config

include $(topdir)/build/platforms/$(BUILD_PLATFORM).make

PROFILE_PLATFORM = $(if $(PLATFORMS),$(if $(filter $(PLATFORMS),$(HOST_PLATFORM)),$(HOST_PLATFORM),$(error Unknown platform "$(HOST_PLATFORM)" for profile "$(PROFILE)")))
PROFILE_DIRECTORY = $(PROFILE)$(if $(PROFILE_PLATFORM),-$(PROFILE_PLATFORM))

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

ifneq ($(or $(ALWAYS_AOT_BCL), $(ALWAYS_AOT_TESTS)),)
TOP_LEVEL_DO = do-all-aot
else
TOP_LEVEL_DO = do-all
endif

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

STD_TARGETS = test xunit-test run-test run-xunit-test run-test-ondotnet clean install uninstall doc-update

$(STD_TARGETS): %: do-%

ifdef PLATFORM_AOT_SUFFIX

do-all-aot:
	$(MAKE) do-all TOP_LEVEL_DO=do-all
	$(MAKE) aot-all-profile

# When we recursively call $(MAKE) aot-all-profile
# we will have created this directory, and so will
# be able to evaluate the .dylibs to make
ifneq ("$(wildcard $(topdir)/class/lib/$(PROFILE))","")

ifdef ALWAYS_AOT_BCL
AOT_PROFILE_ASSEMBLIES := $(sort $(patsubst .//%,%,$(filter-out %.dll.dll %.exe.dll %bare% %plaincore% %secxml% %Facades% %ilasm%,$(filter %.dll %.exe,$(wildcard $(topdir)/class/lib/$(PROFILE)/*)))))
AOT_PROFILE_ASSEMBLIES_OUT := $(patsubst %,%$(PLATFORM_AOT_SUFFIX),$(AOT_PROFILE_ASSEMBLIES))

AOT_PROFILE_FACADES := $(sort $(patsubst .//%,%,$(filter-out %.dll.dll %.exe.dll %bare% %plaincore% %secxml% %Facades% %ilasm%,$(filter %.dll %.exe,$(wildcard $(topdir)/class/lib/$(PROFILE)/Facades/*)))))
AOT_PROFILE_FACADES_OUT := $(patsubst %,%$(PLATFORM_AOT_SUFFIX),$(AOT_PROFILE_FACADES))
endif

ifdef ALWAYS_AOT_TESTS
AOT_PROFILE_TESTS := $(sort $(patsubst .//%,%,$(filter-out %.dll.dll %.exe.dll %bare% %plaincore% %secxml% %Facades% %ilasm%,$(filter %.dll %.exe,$(wildcard $(topdir)/class/lib/$(PROFILE)/tests/*)))))
AOT_PROFILE_TESTS_OUT := $(patsubst %,%$(PLATFORM_AOT_SUFFIX),$(AOT_PROFILE_TESTS))
endif

# This can run in parallel
.PHONY: aot-all-profile
ifdef AOT_BUILD_FLAGS
aot-all-profile: $(AOT_PROFILE_ASSEMBLIES_OUT) $(AOT_PROFILE_TESTS_OUT) $(AOT_PROFILE_FACADES_OUT)
else
aot-all-profile:
	echo AOT_BUILD_FLAGS not set, skipping AOT.
endif

%.dll$(PLATFORM_AOT_SUFFIX): %.dll
	@ mkdir -p $<_bitcode_tmp
	$(Q_AOT) MONO_PATH="$(topdir)/class/lib/$(PROFILE)" $(RUNTIME) $(RUNTIME_FLAGS) $(AOT_BUILD_FLAGS),temp-path=$<_bitcode_tmp $<
	@ rm -rf $<_bitcode_tmp

%.exe$(PLATFORM_AOT_SUFFIX): %.exe
	@ mkdir -p $<_bitcode_tmp
	$(Q_AOT) MONO_PATH="$(topdir)/class/lib/$(PROFILE)" $(RUNTIME) $(RUNTIME_FLAGS) $(AOT_BUILD_FLAGS),temp-path=$<_bitcode_tmp $<
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
PROFILE_SUBDIRS = $(or $($(PROFILE)_SUBDIRS),$(SUBDIRS))

# These subdirs can be built in parallel
PROFILE_PARALLEL_SUBDIRS = $(or $($(PROFILE)_PARALLEL_SUBDIRS),$(PARALLEL_SUBDIRS))

ifndef FRAMEWORK_VERSION_MAJOR
FRAMEWORK_VERSION_MAJOR = $(basename $(FRAMEWORK_VERSION))
endif

%-recursive:
	@set . $$MAKEFLAGS; \
	case $$2 in --unix) shift ;; esac; \
	case $$2 in *=*) dk="exit 1" ;; *k*) dk=: ;; *) dk="exit 1" ;; esac; \
	final_exit=:; \
	$(foreach subdir,$(PROFILE_SUBDIRS),$(MAKE) -C $(subdir) $* || { final_exit="exit 1"; $$dk; };) \
	$(if $(PROFILE_PARALLEL_SUBDIRS), \
		$(if $(filter $*,all), \
			$(MAKE) $(PROFILE_PARALLEL_SUBDIRS) ENABLE_PARALLEL_SUBDIR_BUILD=1 || { final_exit="exit 1"; $$dk; };, \
			$(foreach subdir,$(PROFILE_PARALLEL_SUBDIRS),$(MAKE) -C $(subdir) $* || { final_exit="exit 1"; $$dk; };))) \
	$(if $(FACADES_FOLDER), \
		$(MAKE) -C $(FACADES_FOLDER) $* || { final_exit="exit 1"; $$dk; }; \
	) \
	$$final_exit

ifdef ENABLE_PARALLEL_SUBDIR_BUILD
.PHONY: $(PROFILE_PARALLEL_SUBDIRS)
$(PROFILE_PARALLEL_SUBDIRS):
	@set . $$MAKEFLAGS; \
	$(MAKE) -C $@ all
endif

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

#
# This should track dependencies better but the variable can be defined in any
# Makefile dependency. On top of that we should also regenerate when any of the
# PROFILE_PARALLEL_SUBDIRS Makefile's are changed
#
$(dep_dirs): Makefile
	$(if $(V),@echo "Creating $(abspath $@)...",)
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

ifndef DIST_SUBDIRS
DIST_SUBDIRS = $(SUBDIRS) $(DIST_ONLY_SUBDIRS)
endif
dist-recursive: dist-local
	@case '$(distdir)' in [\\/$$]* | ?:[\\/]* ) reldir='$(distdir)' ;; *) reldir='../$(distdir)' ;; esac ; \
	list='$(DIST_SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) distdir=$$reldir/$$d $@) || exit 1 ; \
	done

# function to dist files in groups of 100 entries to make sure we don't exceed shell char limits
define distfilesingroups
for f in $(wordlist 1, 100, $(1)) ; do \
	dest=`dirname "$(distdir)/$$f"` ; \
	$(MKINSTALLDIRS) $$dest && cp -p "$$f" $$dest || exit 1 ; \
done
$(if $(word 101, $(1)), $(call distfilesingroups, $(wordlist 101, $(words $(1)), $(1))))
endef

# The following target can be used like
#
#   dist-local: dist-default
#	... additional commands ...
#
# Notes:
#  1. we invert the test here to not end in an error if ChangeLog doesn't exist.
#  2. we error out if we try to dist a nonexistant file.
#  3. we pick up Makefile
dist-default:
	-mkdir -p $(distdir)
	test '!' -f ChangeLog || cp ChangeLog $(distdir)
	$(call distfilesingroups, Makefile $(DISTFILES))
	if test -d Documentation ; then \
		find . -name '*.xml' > .files ; \
		tar cTf .files - | (cd $(distdir); tar xf -) ; \
		rm .files ; \
	fi

%/.stamp:
	$(MKINSTALLDIRS) $(@D)
	touch $@

## Documentation stuff

Q_MDOC =$(if $(V),,@echo "MDOC    [$(PROFILE_DIRECTORY)] $(notdir $(@))";)
MDOC   =$(Q_MDOC) MONO_PATH="$(topdir)/class/lib/$(PROFILE_DIRECTORY)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/$(PROFILE_DIRECTORY)/mdoc.exe

Q_MDOC_UP=$(if $(V),,@echo "MDOC-UP [$(PROFILE_DIRECTORY)] $(notdir $(@))";)
MDOC_UP  =$(Q_MDOC_UP) MONO_PATH="$(topdir)/class/lib/$(PROFILE_DIRECTORY)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/$(PROFILE_DIRECTORY)/mdoc.exe update --delete -o Documentation/en
