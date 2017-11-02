#! -*- makefile -*-

BOOTSTRAP_PROFILE = build

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_CSC)
MCS = $(BOOTSTRAP_MCS)

profile-check:
	@:

DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

PROFILE_MCS_FLAGS = \
	-d:NET_1_1 \
	-d:NET_2_0 \
	-d:NET_2_1 \
	-d:NET_3_5 \
	-d:NET_4_0 \
	-d:NET_4_5 \
	-d:MONO \
	-d:UNITY \
	-d:UNITY_AOT \
	-d:MOBILE,MOBILE_LEGACY \
	-d:FULL_AOT_DESKTOP	\
	-d:FULL_AOT_RUNTIME \
	-d:DISABLE_REMOTING \
	-d:WIN_PLATFORM \
	-nowarn:1699 \
	-nostdlib \
	$(DEFAULT_REFERENCES) \
	$(PLATFORM_DEBUG_FLAGS)

FRAMEWORK_VERSION = 2.1

NO_INSTALL = yes
AOT_FRIENDLY_PROFILE = yes
#ALWAYS_AOT = yes
MOBILE_PROFILE = yes
NO_TEST = yes
#NO_SIGN_ASSEMBLY = yes
