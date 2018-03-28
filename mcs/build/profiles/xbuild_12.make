# -*- makefile -*-

include $(topdir)/build/profiles/net_4_x.make

PLATFORMS:=

PARENT_PROFILE = ../net_4_x$(if $(PROFILE_PLATFORM),-$(PROFILE_PLATFORM))/
DEFAULT_REFERENCES = ../net_4_x/mscorlib
PROFILE_MCS_FLAGS += -d:XBUILD_12

XBUILD_VERSION = 12.0
