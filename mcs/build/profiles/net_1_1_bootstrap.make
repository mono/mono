# -*- makefile -*-

ifdef PLATFORM_MONO_NATIVE
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/basic:$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/basic/mcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MCS)
MBAS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MBAS)
else
BOOTSTRAP_MCS = $(EXTERNAL_MCS)
MCS = $(PLATFORM_RUNTIME) $(EXTERNAL_MCS) /lib:$(topdir)/class/lib/$(PROFILE)
MBAS = $(PLATFORM_RUNTIME) $(EXTERNAL_MBAS) /lib:$(topdir)/class/lib/$(PROFILE)
endif

NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1
PROFILE_MBAS_FLAGS = -d:NET_1_1 -d:ONLY_1_1

install-local: no-install
no-install:
	exit 1
