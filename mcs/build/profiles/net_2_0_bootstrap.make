# -*- makefile -*-

#
# Note that we're using the .NET 1.1 MCS but MONO_PATH points to the net_2_0_bootstrap directory.
# We do it this way to get assembly version references right.
#
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:BOOTSTRAP_NET_2_0
FRAMEWORK_VERSION = 2.0
NO_SIGN_ASSEMBLY = yes
