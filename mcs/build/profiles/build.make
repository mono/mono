# -*- makefile -*-

BOOTSTRAP_PROFILE = basic
BUILD_TOOLS_PROFILE = basic

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" CSC_SDK_PATH_DISABLED= $(INTERNAL_CSC)
MCS = $(BOOTSTRAP_MCS)

# nuttzing!

profile-check:
	@:

DEFAULT_REFERENCES = -r:mscorlib.dll
PROFILE_MCS_FLAGS = -d:NET_4_0 -d:NET_4_5 -d:MONO -d:DISABLE_CAS_USE -nowarn:1699 -nostdlib -lib:$(topdir)/class/lib/$(PROFILE) $(DEFAULT_REFERENCES)

NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

FRAMEWORK_VERSION = 4.5
