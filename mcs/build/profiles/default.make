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

ifdef PLATFORM_MONO_NATIVE
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap:$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/net_1_1_bootstrap/mcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MCS)
MBAS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MBAS)
else
BOOTSTRAP_MCS = $(EXTERNAL_MCS)
MCS = $(PLATFORM_RUNTIME) $(BOOTSTRAP_MCS) /lib:$(topdir)/class/lib/$(PROFILE)
MBAS = $(PLATFORM_RUNTIME) $(BOOTSTRAP_MBAS) /lib:$(topdir)/class/lib/$(PROFILE)
endif

# nuttzing!

profile-check:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1
PROFILE_MBAS_FLAGS = -d:NET_1_1 -d:ONLY_1_1
FRAMEWORK_VERSION = 1.0
