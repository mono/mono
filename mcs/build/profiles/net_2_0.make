# -*- makefile -*-
#
# The default 'bootstrap' profile -- builds so that we link against
# the libraries as we build them.
#
# We use the platform's native C# runtime and compiler if possible.

# Note that we have sort of confusing terminology here; BOOTSTRAP_MCS
# is what allows us to bootstrap ourselves, but when we are bootstrapping,
# we use INTERNAL_MCS.

# When bootstrapping, compile against our new assemblies.
# (MONO_PATH doesn't just affect what assemblies are loaded to
# run the compiler; /r: flags are by default loaded from whatever's
# in the MONO_PATH too).

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)

# nuttzing!

profile-check:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -2
FRAMEWORK_VERSION = 2.0
