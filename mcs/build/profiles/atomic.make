# -*- Makefile -*-
#
# The 'atomic' profile.

# In this profile we compile everything relative to the already-installed
# runtime, so we use the bootstrap (external) compiler for everything and
# don't set MONO_PATH.
#
# (So the libraries are compiled and installed atomically, not incrementally.)

MCS = $(BOOTSTRAP_MCS)

# Causes some build errors
#PROFILE_MCS_FLAGS = /d:NET_1_1 /lib:$(prefix)/lib

# Get our installed libraries (an issue on windows)

PROFILE_MCS_FLAGS = /lib:$(prefix)/lib

# Check that installed libraries even exist.

profile-check:
	@if test '!' -f $(prefix)/lib/I18N.dll ; then \
	    echo ; \
	    echo "$(prefix)/lib/I18N.dll does not exist." ; \
	    echo ; \
	    echo "This probably means that you are building from a miniature" ; \
	    echo "distribution of MCS or don't yet have an installed MCS at all." ; \
	    echo "The current build profile needs a complete installation of" ; \
	    echo "MCS to build against; you need to build using the default" ; \
	    echo "profile. Use this command:" ; \
	    echo ; \
	    echo "    $(MAKE) PROFILE=default" ; \
	    echo ; \
	    exit 1 ; \
	fi

# Exciting, no?
