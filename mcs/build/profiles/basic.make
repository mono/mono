# -*- makefile -*-

MCS = false

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -d:BOOTSTRAP_WITH_OLDLIB
USE_BOOT_COMPILE = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

profile-check:
	@:

install-local: no-install
no-install:
	exit 1
