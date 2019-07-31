#! -*- makefile -*-

include $(topdir)/build/profiles/testing_aot_common.make

PROFILE_MCS_FLAGS += \
	-d:FULL_AOT_DESKTOP \
	-d:FULL_AOT_RUNTIME \
	-d:DISABLE_COM

AOT_FRIENDLY_PROFILE = yes
NO_VTS_TEST = yes
NO_SRE = yes

ALWAYS_AOT_BCL = yes
ALWAYS_AOT_TESTS = yes

PROFILE_TEST_HARNESS_EXCLUDES += SRE
