#! -*- makefile -*-

include $(topdir)/build/profiles/testing_aot_common.make

PROFILE_MCS_FLAGS += \
	-d:MOBILE_DYNAMIC

MOBILE_DYNAMIC = yes
