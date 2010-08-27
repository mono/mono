# -*- makefile -*-

INTERNAL_GMCS = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_2_0/gmcs.exe

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)
# nuttzing!

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_3_5
FRAMEWORK_VERSION = 3.5

TEST_HARNESS = $(topdir)/class/lib/net_2_0/nunit-console.exe
TEST_MONO_PATH = $(topdir)/class/lib/net_2_0

