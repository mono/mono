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
Q_MCS=$(if $(V),,@echo "MCS     [$(PROFILE)] $(notdir $(@))";)

ifndef BUILD_TOOLS_PROFILE
BUILD_TOOLS_PROFILE = build
endif

USE_MCS_FLAGS = /codepage:$(CODEPAGE) $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS) $(MCS_FLAGS)
USE_MBAS_FLAGS = /codepage:$(CODEPAGE) $(LOCAL_MBAS_FLAGS) $(PLATFORM_MBAS_FLAGS) $(PROFILE_MBAS_FLAGS) $(MBAS_FLAGS)
USE_CFLAGS = $(LOCAL_CFLAGS) $(CFLAGS) $(CPPFLAGS)
CSCOMPILE = $(Q_MCS) $(MCS) $(USE_MCS_FLAGS)
BASCOMPILE = $(MBAS) $(USE_MBAS_FLAGS)
CCOMPILE = $(CC) $(USE_CFLAGS)
BOOT_COMPILE = $(Q_MCS) $(BOOTSTRAP_MCS) $(USE_MCS_FLAGS)
INSTALL = $(SHELL) $(topdir)/../mono/install-sh
INSTALL_DATA = $(INSTALL) -c -m 644
INSTALL_BIN = $(INSTALL) -c -m 755
INSTALL_LIB = $(INSTALL_BIN)
MKINSTALLDIRS = $(SHELL) $(topdir)/mkinstalldirs
INTERNAL_MBAS = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/mbas/mbas.exe
INTERNAL_GMCS = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/mcs.exe
INTERNAL_ILASM = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/ilasm.exe
corlib = mscorlib.dll

INTERNAL_RESGEN = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/resgen.exe
RESGEN = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_RESGEN)

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
export TEST_HARNESS
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

# Default PLATFORM and PROFILE if they're not already defined.

ifndef PLATFORM
ifeq ($(OS),Windows_NT)
PLATFORM = win32
else
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
-include $(topdir)/build/config.make

ifdef BCL_OPTIMIZE
PROFILE_MCS_FLAGS += -optimize
endif

ifdef OVERRIDE_TARGET_ALL
all: all.override
else
all: do-all
endif

ifdef NO_INSTALL
GACUTIL = :
else
gacutil = $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/gacutil.exe
GACUTIL = MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(gacutil)
endif

STD_TARGETS = test run-test run-test-ondotnet clean install uninstall doc-update

$(STD_TARGETS): %: do-%

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
	$$final_exit

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
# net_2_0 is needed because monodoc is only compiled in that profile
MDOC   =$(Q_MDOC) MONO_PATH="$(topdir)/class/lib/$(DEFAULT_PROFILE)$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/$(DEFAULT_PROFILE)/mdoc.exe

