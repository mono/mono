#! -*- makefile -*-

my_runtime = $(RUNTIME) $(RUNTIME_FLAGS) --security=temporary-smcs-hack
INTERNAL_SMCS = $(my_runtime) $(topdir)/class/lib/$(PROFILE)/smcs.exe

BOOTSTRAP_PROFILE = monotouch_bootstrap

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(my_runtime) $(topdir)/class/lib/$(BOOTSTRAP_PROFILE)/smcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_SMCS)

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_2_1 -d:MOBILE -d:MONOTOUCH
FRAMEWORK_VERSION = 2.1
NO_TEST = yes

# the tuner takes care of the install
NO_INSTALL = yes
