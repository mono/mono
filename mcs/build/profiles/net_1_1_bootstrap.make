# -*- makefile -*-

#BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/basic:$$MONO_PATH" $(INTERNAL_MCS)
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MCS)
MBAS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MBAS)

NO_SIGN_ASSEMBLY = yes

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1
PROFILE_MBAS_FLAGS = -d:NET_1_1 -d:ONLY_1_1

install-local: no-install
no-install:
	exit 1
