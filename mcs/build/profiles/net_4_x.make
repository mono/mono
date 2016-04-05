# -*- makefile -*-

BOOTSTRAP_PROFILE = build

# Using CSC_SDK_PATH_DISABLED for sanity check that all references have path specified

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" CSC_SDK_PATH_DISABLED= $(INTERNAL_CSC)
MCS = $(BOOTSTRAP_MCS)

# nuttzing!

profile-check:
	@:

DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll
PROFILE_MCS_FLAGS = -d:NET_4_0 -d:NET_4_5 -d:NET_4_6 -d:MONO  -nowarn:1699 -nostdlib $(DEFAULT_REFERENCES) $(PLATFORM_DEBUG_FLAGS)

FRAMEWORK_VERSION = 4.5
XBUILD_VERSION = 4.0
