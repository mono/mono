# -*- makefile -*-

INTERNAL_DMCS = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/dmcs.exe

BOOTSTRAP_PROFILE = net_4_0_bootstrap

BOOTSTAP_DMCS = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/dmcs.exe
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(BOOTSTAP_DMCS)
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_DMCS)

# nuttzing!

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_3_0 -d:NET_3_5 -d:NET_4_0 -nowarn:1699
FRAMEWORK_VERSION = 4.0
