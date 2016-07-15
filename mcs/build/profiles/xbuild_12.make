# -*- makefile -*-

include $(topdir)/build/profiles/net_4_x.make

PARENT_PROFILE = ../net_4_x/
DEFAULT_REFERENCES = -r:$(topdir)/class/lib/net_4_x/mscorlib.dll
PROFILE_MCS_FLAGS := $(PROFILE_MCS_FLAGS) -d:XBUILD_12

XBUILD_VERSION = 12.0
