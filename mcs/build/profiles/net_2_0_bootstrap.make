# -*- makefile -*-

#
# Note that we're using the .NET 1.1 MCS but MONO_PATH points to the net_2_0_bootstrap directory.
# We do it this way to get assembly version references right.
#
BOOTSTRAP_PROFILE = net_1_1_bootstrap
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/mcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/gmcs.exe

profile-check: 

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:BOOTSTRAP_NET_2_0
FRAMEWORK_VERSION = 2.0
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

