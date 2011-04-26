#! -*- makefile -*-

BOOTSTRAP_PROFILE = build

BOOTSTRAP_MCS = $(INTERNAL_GMCS)
MCS = $(BOOTSTRAP_MCS)

profile-check: $(depsdir)/.stamp
	@:

DEFAULT_REFERENCES = -r:mscorlib.dll
PROFILE_MCS_FLAGS = -lib:$(topdir)/class/lib/moonlight_raw -d:NET_1_1 -d:NET_2_0 -d:NET_2_1 -d:MOONLIGHT -d:SILVERLIGHT -nowarn:1699 -nostdlib -lib:$(topdir)/class/lib/$(PROFILE) $(DEFAULT_REFERENCES)
SN = sn
FRAMEWORK_VERSION = 2.1
NO_TEST = yes

# the tuner takes care of the install
NO_INSTALL = yes
