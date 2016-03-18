#! -*- makefile -*-

BOOTSTRAP_PROFILE = build

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" CSC_SDK_PATH_DISABLED= $(INTERNAL_CSC)
MCS = $(BOOTSTRAP_MCS)

# Use system resgen as we don't want local System.Windows.Forms dependency
RESGEN = resgen2

profile-check:
	@:

DEFAULT_REFERENCES = -r:mscorlib.dll

PROFILE_MCS_FLAGS = \
	-d:NET_1_1 \
	-d:NET_2_0 \
	-d:NET_2_1 \
	-d:NET_3_5 \
	-d:NET_4_0 \
	-d:NET_4_5 \
	-d:MONO \
	-d:DISABLE_CAS_USE \
	-d:MOBILE,MOBILE_LEGACY \
	-d:MOBILE_DYNAMIC \
	-d:MONODROID \
	-d:ANDROID \
	-nowarn:1699 \
	-nostdlib \
	-lib:$(topdir)/class/lib/$(PROFILE) \
	$(DEFAULT_REFERENCES) \
	$(PLATFORM_DEBUG_FLAGS)

FRAMEWORK_VERSION = 2.1
NO_TEST = yes

# the tuner takes care of the install
NO_INSTALL = yes
MOBILE_DYNAMIC = yes
MOBILE_PROFILE = yes
