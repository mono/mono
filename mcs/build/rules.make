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

VERSION = 0.25.99

USE_MCS_FLAGS = $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS) $(MCS_FLAGS)
USE_CFLAGS = $(LOCAL_CFLAGS) $(CFLAGS)
CSCOMPILE = $(MCS) $(USE_MCS_FLAGS)
CCOMPILE = $(CC) $(USE_CFLAGS)
BOOT_COMPILE = $(BOOTSTRAP_MCS) $(USE_MCS_FLAGS)
INSTALL_DATA = $(INSTALL) -m 644
INSTALL_BIN = $(INSTALL) -m 755
INSTALL_LIB = $(INSTALL_BIN)
MKINSTALLDIRS = $(SHELL) $(topdir)/mkinstalldirs
INTERNAL_MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
corlib = corlib.dll

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

# Get this so the platform.make platform-check rule doesn't become the
# default target

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

# Simple rules

%-recursive:
	@list='$(SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) $*) || exit 1 ; \
	done

# note: dist-local dep, extra subdirs, $* has become $@

dist-recursive: dist-local
	@list='$(SUBDIRS) $(DIST_ONLY_SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) $@) || exit 1 ; \
	done

# We do this manually to not have a make[1]: blah message (That is,
# instead of using a '%: %-recursive %-local' construct.)
#
# Let the makefile override these for special situations (running checks
# in the toplevel makefile, or a directory that needs to be built before
# its subdirectories).

ifndef OVERRIDE_BARE_TARGETS

all: all-recursive all-local

install: install-recursive install-local

test: test-recursive test-local

run-test: run-test-recursive test-local run-test-local

clean: clean-recursive clean-local

endif

# Can only do this from the top dir
# ## dist: dist-recursive dist-local

# We invert the test here to not end in an error
# if ChangeLog doesn't exist.
#
# Note that we error out if we try to dist a nonexistant
# file. Seems reasonable to me.

dist-default:
	-mkdir $(distdir)
	test '!' -f ChangeLog || cp ChangeLog $(distdir)
	for f in Makefile $(DISTFILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

# Useful

withmcs:
	$(MAKE) MCS='$(INTERNAL_MCS)' BOOTSTRAP_MCS='$(INTERNAL_MCS)' all
