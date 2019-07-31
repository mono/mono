#! -*- makefile -*-

include $(topdir)/build/profiles/testing_aot_common.make

PROFILE_MCS_FLAGS += \
	-d:FULL_AOT_INTERP \
	-d:DISABLE_COM

ALWAYS_AOT_BCL = yes
