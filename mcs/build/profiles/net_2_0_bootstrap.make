# -*- makefile -*-

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)

profile-check:
	test -f $(topdir)/mcs/mcs.exe

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:BOOTSTRAP_NET_2_0 -langversion:default
FRAMEWORK_VERSION = 2.0
NO_SIGN_ASSEMBLY = yes
