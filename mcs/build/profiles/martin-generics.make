# -*- makefile -*-
#
# Generics support: this is highly experimental and unstable.
#
# As the name of this profile already says: it's basically just for me.
# February 17, 2004  Martin Baulig <martin@ximian.com>
#

ifdef PLATFORM_MONO_NATIVE
MCS = MONO_PATH="$(topdir)/class/lib:$$MONO_PATH" $(RUNTIME) --debug $(topdir)/gmcs/gmcs.exe
BOOTSTRAP_MCS = $(MCS)
TEST_RUNTIME = MONO_PATH=".:$$MONO_PATH" $(RUNTIME) --debug
else
MCS = $(PLATFORM_RUNTIME) $(BOOTSTRAP_MCS) /lib:$(topdir)/class/lib
endif

# nuttzing!

profile-check:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_1_2 -d:GENERICS -2
