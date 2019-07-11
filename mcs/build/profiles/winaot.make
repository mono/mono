#! -*- makefile -*-

include $(topdir)/build/profiles/winaot_common.make

PROFILE_MCS_FLAGS += \
	-d:FULL_AOT_DESKTOP	\
	-d:FULL_AOT_RUNTIME

ALWAYS_AOT_TESTS = yes
NO_VTS_TEST = yes
NO_SRE = yes

PROFILE_TEST_HARNESS_EXCLUDES = MobileNotWorking PKITS SRE NotWorkingLinqInterpreter