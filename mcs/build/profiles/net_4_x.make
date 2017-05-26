# -*- makefile -*-

PLATFORMS = darwin linux win32

# nuttzing!

profile-check:
	@:

DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll
PROFILE_MCS_FLAGS = -d:NET_4_0 -d:NET_4_5 -d:NET_4_6 -d:MONO -d:WIN_PLATFORM -d:MULTIPLEX_OS -nowarn:1699 -nostdlib $(DEFAULT_REFERENCES) $(PLATFORM_DEBUG_FLAGS)

FRAMEWORK_VERSION = 4.5
XBUILD_VERSION = 4.0
MONO_FEATURE_APPLETLS=1
