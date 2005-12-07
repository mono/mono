# -*- makefile -*-
#
# The default 'bootstrap' profile -- builds so that we link against
# the libraries as we build them.
#
# We use the platform's native C# runtime and compiler if possible.

# Note that we have sort of confusing terminology here; BOOTSTRAP_MCS
# is what allows us to bootstrap ourselves, but when we are bootstrapping,
# we use INTERNAL_MCS.

# When bootstrapping, compile against our new assemblies.
# (MONO_PATH doesn't just affect what assemblies are loaded to
# run the compiler; /r: flags are by default loaded from whatever's
# in the MONO_PATH too).

EXTERNAL_MCS = csc.exe
NO_SIGN_ASSEMBLY = yes

#DEXT = pdb

ifdef PLATFORM_MONO_NATIVE
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_1_1_bootstrap/mcs.exe
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)
MBAS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MBAS)
else
BOOTSTRAP_MCS = $(EXTERNAL_MCS)
MCS = $(PLATFORM_RUNTIME) $(EXTERNAL_MCS) /lib:$(topdir)/class/lib/$(PROFILE)
MBAS = $(PLATFORM_RUNTIME) $(EXTERNAL_MBAS) /lib:$(topdir)/class/lib/$(PROFILE)
endif

# nuttzing!

profile-check:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -d:TARGET_JVM -d:JAVA
PROFILE_MBAS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -d:TARGET_JVM -d:JAVA
FRAMEWORK_VERSION = 1.0
CONVERTER_DEBUG_LEVEL = 3
ifeq ($(CONFIG),Release)
PLATFORM_DEBUG_FLAGS = /debug:pdbonly
CONVERTER_DEBUG_LEVEL = 2
endif
library_CLEAN_FILES += $(build_lib:.dll=.jar) $(build_lib:.dll=.pdb)

all-local:
	$(MAKE) $(build_lib:.dll=.jar)

%.jar:%.dll
	converter.exe /debug:$(CONVERTER_DEBUG_LEVEL) $< /out:$@ $(KEY) /lib:$(dir $@)
