# -*- makefile -*-

BOOTSTRAP_PROFILE = build
TOOLS_PROFILE = net_4_5

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)
MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)

INTERNAL_RESGEN = $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(TOOLS_PROFILE)/resgen.exe
RESGEN = MONO_PATH="$(topdir)/class/lib/$(TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_RESGEN)

# nuttzing!

profile-check:
	@:

DEFAULT_REFERENCES = -r:mscorlib.dll
PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_3_0 -d:NET_3_5 -d:NET_4_0 -d:MONO -d:DISABLE_CAS_USE -nowarn:1699,1635 -warn:1 -nostdlib -lib:$(topdir)/class/lib/$(PROFILE) $(DEFAULT_REFERENCES) --metadata-only

FRAMEWORK_VERSION = 4.0
XBUILD_VERSION = 4.0

LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

# Ignore tests on net_4_0 as the 4.0 IL code is never used for running (just for metadata), so it doesn't make sense to execute tests there
NO_TEST = yes