# -*- makefile -*-

BOOTSTRAP_PROFILE = basic

MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)

# nuttzing!

profile-check:
	@:

DEFAULT_REFERENCES = -r:mscorlib.dll
PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_3_5 -nowarn:1699 -nostdlib -lib:$(topdir)/class/lib/$(PROFILE) -lib:$(topdir)/class/lib/net_2_0 $(DEFAULT_REFERENCES)

FRAMEWORK_VERSION = 3.5

TEST_HARNESS = $(topdir)/class/lib/net_2_0/nunit-console.exe
TEST_MONO_PATH = $(topdir)/class/lib/net_2_0

