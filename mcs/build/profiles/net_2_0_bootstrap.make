# -*- makefile -*-

#
# Note that we're using the .NET 1.1 MCS but MONO_PATH points to the net_2_0_bootstrap directory.
# We do it this way to get assembly version references right.
#
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/default$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/default/mcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)

profile-check: 

all-local: $(topdir)/class/lib/$(PROFILE)/mcs.exe $(topdir)/class/lib/$(PROFILE)/mcs.exe.config

$(topdir)/class/lib/$(PROFILE)/mcs.exe: $(topdir)/class/lib/default/mcs.exe
	cp $< $@

$(topdir)/class/lib/$(PROFILE)/mcs.exe.config: $(topdir)/gmcs/gmcs.exe.config
	cp $< $@

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:BOOTSTRAP_NET_2_0
FRAMEWORK_VERSION = 2.0
NO_SIGN_ASSEMBLY = yes

clean-local: clean-profile

clean-profile:
	rm -f $(topdir)/class/lib/$(PROFILE)/mcs.exe
	rm -f $(topdir)/class/lib/$(PROFILE)/mcs.exe.config
