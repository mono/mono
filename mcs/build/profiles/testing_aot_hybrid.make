#! -*- makefile -*-

profile-check:
	@:

DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

PROFILE_MCS_FLAGS = \
	-d:NET_1_1 \
	-d:NET_2_0 \
	-d:NET_2_1 \
	-d:MOBILE,MOBILE_LEGACY \
	-d:MOBILE_DYNAMIC \
	-d:NET_3_5 \
	-d:NET_4_0 \
	-d:NET_4_5 \
	-d:MONO \
	-nowarn:1699 \
	-nostdlib \
	$(DEFAULT_REFERENCES) \
	$(PLATFORM_DEBUG_FLAGS)

FRAMEWORK_VERSION = 2.1

NO_INSTALL = yes
MOBILE_DYNAMIC = yes
MOBILE_PROFILE = yes
NO_CONSOLE = yes
