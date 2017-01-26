# -*- makefile -*-

BOOTSTRAP_PROFILE = build

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_CSC)
MCS = $(BOOTSTRAP_MCS)

# Use system resgen as we don't want local System.Windows.Forms dependency
RESGEN := $(dir $(shell which $(EXTERNAL_MCS)))resgen2

# nuttzing!

profile-check:
	@:

DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll
PROFILE_MCS_FLAGS = \
	-d:NET_1_1 \
	-d:NET_2_0 \
	-d:NET_2_1 \
	-d:NET_3_5 \
	-d:NET_4_0 \
	-d:NET_4_5 \
	-d:MOBILE,MOBILE_LEGACY \
	-d:MONO \
	-d:MONOTOUCH \
	-d:DISABLE_REMOTING \
	-d:DISABLE_COM \
	-d:FEATURE_INTERCEPTABLE_THREADPOOL_CALLBACK \
	-d:NETSTANDARD \
	-nowarn:1699 \
	-nostdlib \
	$(DEFAULT_REFERENCES) \
	$(PLATFORM_DEBUG_FLAGS) \
	$(MONOTOUCH_MCS_FLAGS)

FRAMEWORK_VERSION = 2.1

# This is utility build only
NO_INSTALL = yes
AOT_FRIENDLY_PROFILE = yes
MOBILE_PROFILE = yes
NO_CONSOLE = yes

PROFILE_DISABLE_BTLS=1
