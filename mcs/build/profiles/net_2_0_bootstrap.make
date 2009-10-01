# -*- makefile -*-

#
# Note that we're using the .NET 1.1 MCS but MONO_PATH points to the net_2_0_bootstrap directory.
# We do it this way to get assembly version references right.
#
BOOTSTRAP_PROFILE = basic
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/gmcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/gmcs.exe

profile-check: 

# Don't build with debug symbols (Mono.CompilerServices.SymbolWriter.dll dependency)
PLATFORM_DEBUG_FLAGS =

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0
FRAMEWORK_VERSION = 2.0
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

