# -*- makefile -*-

include $(topdir)/build/profiles/net_4_5.make

PROFILE_MCS_FLAGS := $(PROFILE_MCS_FLAGS) -d:XBUILD_12 -d:XBUILD_14 -d:MONO -d:DISABLE_CAS_USE  -lib:$(topdir)/class/lib/net_4_5

XBUILD_VERSION = 14.0
