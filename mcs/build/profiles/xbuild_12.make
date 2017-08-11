# -*- makefile -*-

include $(topdir)/build/profiles/net_4_x.make

PLATFORMS:=

PARENT_PROFILE_NAME = net_4_x$(if $(PROFILE_PLATFORM),-$(PROFILE_PLATFORM))
PARENT_PROFILE = ../$(PARENT_PROFILE_NAME)/
DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PARENT_PROFILE_NAME)/mscorlib.dll
PROFILE_MCS_FLAGS += -d:XBUILD_12

XBUILD_VERSION = 12.0
