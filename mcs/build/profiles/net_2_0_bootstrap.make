# -*- makefile -*-

#
# Note that we're using the .NET 1.1 MCS but MONO_PATH points to the net_2_0_bootstrap directory.
# We do it this way to get assembly version references right.
#
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)

# Make sure that we're not invoked at the top-level.
profile-check:
	echo "The 'net_2_0_bootstrap' profile is for internal use only"
	exit 1

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:BOOTSTRAP_NET_2_0 -langversion:default
FRAMEWORK_VERSION = 2.0
NO_SIGN_ASSEMBLY = yes
