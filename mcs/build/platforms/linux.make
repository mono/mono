# -*- makefile -*-
#
# Platform-specific makefile rules. This one's for linux.
#

PLATFORM_DEBUG_FLAGS = -g
PLATFORM_MCS_FLAGS = 
PLATFORM_RUNTIME = $(RUNTIME)
PLATFORM_CORLIB = corlib.dll

BOOTSTRAP_MCS = mcs

# Define this if this ever will work on Linux
# PLATFORM_MAKE_CORLIB_CMP = yes

# This is for the security permission attribute problem
# on windows
PLATFORM_TWEAK_CORLIB_SOURCES = cat

# This is for changing / to \ on windows
# Don't define it so we don't needlessly copy the sources
# file. This command is handy for testing:
#
# PLATFORM_CHANGE_SEPARATOR_CMD=sed -e 's,/,/./,g'

hidden_prefix = .
hidden_suffix = 

platform-check:
	@if ! type $(BOOTSTRAP_MCS) >/dev/null 2>&1 ; then \
	    echo "*** You need a C# compiler installed to build MCS." ; \
	    echo "*** Read README.building for information on how to bootstrap" ; \
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
