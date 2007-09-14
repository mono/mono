#! -*- makefile -*-

INTERNAL_SMCS = $(RUNTIME) $(RUNTIME_FLAGS) --security=temporary-smcs-hack $(topdir)/class/lib/net_2_1/smcs.exe

BOOTSTRAP_PROFILE = net_2_1
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_1$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_SMCS)
MCS = MONO_PATH="$(topdir)/class/lib/net_2_1$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_SMCS)

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_2_1
FRAMEWORK_VERSION = 2.1
NO_TEST = yes

# the tuner takes care of the install
#NO_INSTALL = yes
