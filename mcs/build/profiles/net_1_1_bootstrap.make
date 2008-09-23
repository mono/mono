# -*- makefile -*-

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/basic$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/basic/mcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)
MBAS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MBAS)

NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

#
# Copy from rules.make because I don't know how to unset MCS_FLAGS
#
USE_MCS_FLAGS = /codepage:$(CODEPAGE) $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS)

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1
PROFILE_MBAS_FLAGS = -d:NET_1_1 -d:ONLY_1_1
FRAMEWORK_VERSION = 1.0
