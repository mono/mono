# -*- makefile -*-
#
# Platform-specific makefile rules. This one's for linux.
#

PLATFORM_DEBUG_FLAGS = -g
PLATFORM_MCS_FLAGS = 
PLATFORM_RUNTIME = $(RUNTIME)
PLATFORM_CORLIB = mscorlib.dll

BOOTSTRAP_MCS = mcs
RESGEN = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_RESGEN)

PLATFORM_PATH_SEPARATOR = :

# Define this if this ever will work on Linux
# PLATFORM_MAKE_CORLIB_CMP = yes

# This is for changing / to \ on windows
PLATFORM_CHANGE_SEPARATOR_CMD = cat

hidden_prefix = .
hidden_suffix = 

platform-check:
	@set fnord $(BOOTSTRAP_MCS) ; while test "$$#" -gt 2; do case $$2 in *=*) shift ;; *) break ;; esac done ; \
	if type $$2 >/dev/null 2>&1 ; then :; else \
	    echo "*** You need a C# compiler installed to build MCS. (make sure mcs works from the command line)" ; \
	    echo "*** Read INSTALL.txt for information on how to bootstrap" ; \
	    echo "*** a Mono installation." ; \
	    exit 1 ; \
	fi

# I tried this but apparently Make's version strings aren't that
# ... consistent between releases. Whatever.
#
#	@if ! $(MAKE) --version |grep '^GNU Make version 3' 1>/dev/null 2>&1 ; then \
#	    echo "*** You need to build MCS with GNU make. Try \`gmake'" ; \
#	    exit 1 ; \
#	fi
