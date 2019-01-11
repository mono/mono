#! -*- makefile -*-

BOOTSTRAP_PROFILE = build

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_CSC)
MCS = $(BOOTSTRAP_MCS)

profile-check:
	@:

DEFAULT_REFERENCES = mscorlib

PROFILE_MCS_FLAGS = \
	-d:NET_1_1 \
	-d:NET_2_0 \
	-d:NET_2_1 \
	-d:NET_3_5 \
	-d:NET_4_0 \
	-d:NET_4_5 \
	-d:MONO \
	-d:MOBILE,MOBILE_LEGACY \
	-d:FULL_AOT_DESKTOP	\
	-d:FULL_AOT_RUNTIME \
	-d:ORBIS \
	-d:DISABLE_REMOTING \
	-d:DISABLE_COM \
	-nowarn:1699 \
	-nostdlib \
	$(PLATFORM_DEBUG_FLAGS)

API_BIN_PROFILE = build/monotouch
FRAMEWORK_VERSION = 2.1

# the tuner takes care of the install
NO_INSTALL = yes
AOT_FRIENDLY_PROFILE = yes
ALWAYS_AOT_BCL = yes
ALWAYS_AOT_TESTS = yes
MOBILE_PROFILE = yes
NO_VTS_TEST = yes
NO_CONSOLE = yes
PROFILE_DISABLE_BTLS = yes
NO_SRE=1

# Note need for trailing comma. If you add, keep it
PROFILE_TEST_HARNESS_EXCLUDES = MobileNotWorking,PKITS,
