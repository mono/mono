# -*- makefile -*-

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"
with_mono_path_monolite = MONO_PATH="$(topdir)/class/lib/monolite$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

monolite_flag := $(depsdir)/use-monolite
use_monolite := $(wildcard $(monolite_flag))

MONOLITE_MCS = $(topdir)/class/lib/monolite/basic.exe

ifdef use_monolite
PROFILE_RUNTIME = $(with_mono_path_monolite) $(RUNTIME)
BOOTSTRAP_MCS = $(PROFILE_RUNTIME) $(RUNTIME_FLAGS) $(MONOLITE_MCS)
else
PROFILE_RUNTIME = $(EXTERNAL_RUNTIME)
BOOTSTRAP_MCS = $(EXTERNAL_MCS)
endif

MCS = $(with_mono_path) $(INTERNAL_GMCS)

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:BOOTSTRAP_BASIC -nowarn:1699
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes
FRAMEWORK_VERSION = 2.0

# Compiler all using same bootstrap compiler
LIBRARY_COMPILE = $(BOOT_COMPILE)

# Verbose basic only
# V = 1

#
# Copy from rules.make because I don't know how to unset MCS_FLAGS
#
USE_MCS_FLAGS = /codepage:$(CODEPAGE) $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS)

.PHONY: profile-check do-profile-check
profile-check:
	@:

ifeq (.,$(thisdir))
all-recursive: do-profile-check
all-local: post-profile-cleanup
clean-local: clean-profile
endif

clean-profile:
	-rm -f $(PROFILE_EXE) $(PROFILE_OUT) $(monolite_flag)

post-profile-cleanup:
	@rm -f $(monolite_flag)

PROFILE_EXE = $(depsdir)/basic-profile-check.exe
PROFILE_OUT = $(PROFILE_EXE:.exe=.out)

MAKE_Q=$(if $(V),,-s)

do-profile-check: $(depsdir)/.stamp
	@ok=:; \
	rm -f $(PROFILE_EXE) $(PROFILE_OUT); \
	$(MAKE) $(MAKE_Q) $(PROFILE_OUT) || ok=false; \
	rm -f $(PROFILE_EXE) $(PROFILE_OUT); \
	if $$ok; then :; else \
	    if test -f $(MONOLITE_MCS); then \
		$(MAKE) -s do-profile-check-monolite ; \
	    else \
		echo "*** The compiler '$(BOOTSTRAP_MCS)' doesn't appear to be usable." 1>&2; \
                echo "*** You need Mono version 2.4 or better installed to build MCS" 1>&2 ; \
                echo "*** Read INSTALL.txt for information on how to bootstrap a Mono installation." 1>&2 ; \
	        exit 1; fi; fi


ifdef use_monolite

do-profile-check-monolite:
	echo "*** The contents of your 'monolite' directory may be out-of-date" 1>&2
	echo "*** You may want to try 'make get-monolite-latest'" 1>&2
	rm -f $(monolite_flag)
	exit 1

else

do-profile-check-monolite: $(depsdir)/.stamp
	echo "*** The compiler '$(BOOTSTRAP_MCS)' doesn't appear to be usable." 1>&2
	echo "*** Trying the 'monolite' directory." 1>&2
	echo dummy > $(monolite_flag)
	$(MAKE) do-profile-check

endif

$(PROFILE_EXE): $(topdir)/build/common/basic-profile-check.cs
	$(BOOTSTRAP_MCS) /warn:0 /out:$@ $<
	echo -n "Bootstrap compiler: " 1>&2
	$(BOOTSTRAP_MCS) --version 1>&2

$(PROFILE_OUT): $(PROFILE_EXE)
	$(PROFILE_RUNTIME) $< > $@ 2>&1
