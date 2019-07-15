#! -*- makefile -*-

include $(topdir)/build/profiles/winaot_common.make

PROFILE_MCS_FLAGS += \
	-d:FULL_AOT_INTERP \
	-d:DISABLE_SECURITY \
	-d:DISABLE_REMOTING

DISABLE_REMOTING = yes
NO_MULTIPLE_APPDOMAINS = yes

PROFILE_TEST_HARNESS_EXCLUDES := MobileNotWorking PKITS
