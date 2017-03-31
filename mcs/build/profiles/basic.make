# -*- makefile -*-

with_mono_path_monolite = MONO_PATH="$(topdir)/class/lib/monolite$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/monolite/Facades$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

monolite_flag := $(depsdir)/use-monolite
use_monolite := $(wildcard $(monolite_flag))

MONOLITE_MSCORLIB = $(topdir)/class/lib/monolite/mscorlib.dll

ifdef use_monolite
ifdef MCS_MODE
	CSC_LOCATION = $(topdir)/class/lib/monolite/mcs.exe
endif

PROFILE_RUNTIME = $(with_mono_path_monolite) $(RUNTIME)
BOOTSTRAP_MCS = $(PROFILE_RUNTIME) $(RUNTIME_FLAGS) $(CSC_LOCATION)

else
PROFILE_RUNTIME = $(EXTERNAL_RUNTIME)
ifdef MCS_MODE
	BOOTSTRAP_MCS = mcs
else
	BOOTSTRAP_MCS = $(PROFILE_RUNTIME) $(RUNTIME_FLAGS) $(CSC_LOCATION)
endif
endif

MCS = $(BOOTSTRAP_MCS)

DEFAULT_REFERENCES = -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

PROFILE_MCS_FLAGS = -d:NET_4_0 -d:NET_4_5 -d:MONO -d:WIN_PLATFORM -d:BOOTSTRAP_BASIC -nowarn:1699 -nostdlib $(DEFAULT_REFERENCES)
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes
FRAMEWORK_VERSION = 4.0

# Compiler all using same bootstrap compiler
LIBRARY_COMPILE = $(BOOT_COMPILE)

# Verbose basic only
# V = 1

#
# Copy from rules.make because I don't know how to unset MCS_FLAGS
#
USE_MCS_FLAGS = /codepage:$(CODEPAGE) /nologo /noconfig $(LOCAL_MCS_FLAGS) $(PLATFORM_MCS_FLAGS) $(PROFILE_MCS_FLAGS)

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
	if [ -z '$(MAKE_Q)' ] && [ -n '$(PROFILE_RUNTIME)' ]; then $(PROFILE_RUNTIME) --version; fi; \
	$(MAKE) $(MAKE_Q) $(PROFILE_OUT) || ok=false; \
	if $$ok; then rm -f $(PROFILE_EXE) $(PROFILE_OUT); else \
	    if test ! -s $(MONOLITE_MSCORLIB); then \
			$(MAKE) $(MAKE_Q) do-get-monolite ; \
		fi; \
	    if test -f $(MONOLITE_MSCORLIB); then \
		$(MAKE) $(MAKE_Q) do-profile-check-monolite ; \
	    else \
		echo "*** The runtime '$(PROFILE_RUNTIME)' doesn't appear to be usable." 1>&2; \
                echo "*** You need Mono version 4.8 or better installed to build MCS" 1>&2 ; \
                echo "*** Check mono README for information on how to bootstrap a Mono installation." 1>&2 ; \
	        exit 1; fi; fi


ifdef use_monolite

do-get-monolite:

do-profile-check-monolite:
	@echo "*** The contents of your 'monolite' directory may be out-of-date" 1>&2
	@echo "*** You may want to try 'make get-monolite-latest'" 1>&2
	rm -f $(monolite_flag)
	exit 1

else

do-get-monolite:
	@echo "*** Downloading bootstrap required 'monolite'" 1>&2
	$(MAKE) $(MAKE_Q) -C $(mono_build_root) get-monolite-latest

do-profile-check-monolite: $(depsdir)/.stamp
	@echo "*** The runtime '$(PROFILE_RUNTIME)' doesn't appear to be usable." 1>&2
	@echo "*** Trying the 'monolite' directory." 1>&2
	@echo dummy > $(monolite_flag)
	$(MAKE) do-profile-check

endif

$(PROFILE_EXE): $(topdir)/build/common/basic-profile-check.cs
	$(MAKE) $(MAKE_Q) -C $(topdir)/packages
	$(BOOTSTRAP_MCS) /warn:0 /noconfig /r:System.dll /r:mscorlib.dll /out:$@ $<

$(PROFILE_OUT): $(PROFILE_EXE)
	$(PROFILE_RUNTIME) $< > $@ 2>&1
