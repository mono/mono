# -*- makefile -*-
#
# A bootstrap profile -- builds with the internal MCS, even if
# we're on Windows.

# Note that we have sort of confusing terminology here; BOOTSTRAP_MCS
# is what allows us to bootstrap ourselves, but when we are bootstrapping,
# we use INTERNAL_MCS.

# When bootstrapping, compile against our new assemblies.
# (MONO_PATH doesn't just affect what assemblies are loaded to
# run the compiler; /r: flags are by default loaded from whatever's
# in the MONO_PATH too).

MCS = MONO_PATH="$(topdir)/class/lib:$$MONO_PATH" $(INTERNAL_MCS)

# Causes some build errors
#PROFILE_MCS_FLAGS = /d:NET_1_1
