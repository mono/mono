# -*- makefile -*-
#
# This is the makefile fragment with default rules
# for building things in MCS
#
# To customize the build, you should edit config.make.
# If you need to edit this file, that's a bug; email
# peter@newton.cx about it.

# Some more variables. The leading period in the sed expression prevents
# thisdir = . from being changed into '..' for the toplevel directory.

dots := $(shell echo $(thisdir) |sed -e 's,[^./][^/]*,..,g')
topdir := $(dots)

VERSION = 0.93

USE_MCS_FLAGS = $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS) $(MCS_FLAGS)
USE_MBAS_FLAGS = $(LOCAL_MBAS_FLAGS) $(PLATFORM_MBAS_FLAGS) $(PROFILE_MBAS_FLAGS) $(MBAS_FLAGS)
USE_CFLAGS = $(LOCAL_CFLAGS) $(CFLAGS)
CSCOMPILE = $(MCS) $(USE_MCS_FLAGS)
BASCOMPILE = $(MBAS) $(USE_MBAS_FLAGS)
CCOMPILE = $(CC) $(USE_CFLAGS)
BOOT_COMPILE = $(BOOTSTRAP_MCS) $(USE_MCS_FLAGS)
INSTALL_DATA = $(INSTALL) -m 644
INSTALL_BIN = $(INSTALL) -m 755
INSTALL_LIB = $(INSTALL_BIN)
MKINSTALLDIRS = $(SHELL) $(topdir)/mkinstalldirs
INTERNAL_MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
INTERNAL_MBAS = $(RUNTIME) $(topdir)/mbas/mbas.exe
INTERNAL_GMCS = $(RUNTIME) $(topdir)/gmcs/gmcs.exe
INTERNAL_ILASM = $(RUNTIME) $(topdir)/ilasm/ilasm.exe
INTERNAL_RESGEN = $(RUNTIME) $(topdir)/monoresgen/monoresgen.exe
corlib = mscorlib.dll

depsdir = $(topdir)/build/deps
distdir = $(dots)/$(package)/$(thisdir)

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
PROFILE = default
endif

include $(topdir)/build/profiles/$(PROFILE).make
-include $(topdir)/build/config.make

ifdef OVERRIDE_TARGET_ALL
all: all.override
else
all: all.real
endif

all.real: all-recursive
	$(MAKE) all-local

STD_TARGETS = test run-test run-test-ondotnet clean install uninstall

$(STD_TARGETS): %: %-recursive
	$(MAKE) $@-local

%-recursive:
	@set . $$MAKEFLAGS; \
	case $$2 in --unix) shift ;; esac; \
	case $$2 in *=*) dk="exit 1" ;; *k*) dk=: ;; *) dk="exit 1" ;; esac; \
	list='$(SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) $*) || $$dk ; \
	done

# note: dist-local dep, extra subdirs, we invoke dist-recursive in the subdir too
dist-recursive: dist-local
	@list='$(SUBDIRS) $(DIST_ONLY_SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) $@) || exit 1 ; \
	done

# Can only do this from the top dir
# ## dist: dist-recursive dist-local

# We invert the test here to not end in an error
# if ChangeLog doesn't exist.
#
# Note that we error out if we try to dist a nonexistant
# file. Seems reasonable to me.
#
# Pick up Makefile, makefile, or GNUmakefile

dist-default:
	-mkdir -p $(distdir)
	test '!' -f ChangeLog || cp ChangeLog $(distdir)
	if test -f Makefile; then m=M; fi; \
	if test -f makefile; then m=m; fi; \
	if test -f GNUmakefile; then m=GNUm; fi; \
	for f in $${m}akefile $(DISTFILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

# Useful

withmcs:
	$(MAKE) MCS='$(INTERNAL_MCS)' BOOTSTRAP_MCS='$(INTERNAL_MCS)' all
