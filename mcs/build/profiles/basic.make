# -*- makefile -*-

BOOTSTRAP_MCS = $(EXTERNAL_MCS)
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -d:BOOTSTRAP_WITH_OLDLIB
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

.PHONY: profile-check do-profile-check
profile-check:
	@:

ifeq (.,$(thisdir))
all-recursive: do-profile-check
clean-local: clean-profile
endif

clean-profile:
	-rm -f $(PROFILE_CS) $(PROFILE_EXE) $(PROFILE_OUT)

install-local: no-install
no-install:
	exit 1

PROFILE_CS  = $(topdir)/build/deps/basic-profile-check.cs
PROFILE_EXE = $(PROFILE_CS:.cs=.exe)
PROFILE_OUT = $(PROFILE_CS:.cs=.out)

do-profile-check:
	@ok=:; \
	rm -f $(PROFILE_EXE) $(PROFILE_OUT); \
	$(MAKE) -s $(PROFILE_OUT) || ok=false; \
	rm -f $(PROFILE_EXE) $(PROFILE_OUT); \
	if $$ok; then :; else \
	    echo "*** The compiler '$(EXTERNAL_MCS)' doesn't appear to be usable." 1>&2 ; \
	    if test -f $(topdir)/class/lib/monolite/mcs.exe; then \
	        echo "*** Falling back to using pre-compiled binaries.  Be warned, this may not work." 1>&2 ; \
	        ( cd $(topdir)/class/lib/monolite/ && cp *.exe *.dll ../basic ); \
		( cd $(topdir)/jay && $(MAKE) ); ( cd $(topdir)/mcs && $(MAKE) PROFILE=basic cs-parser.cs ); \
	        touch $(topdir)/class/lib/basic/*; \
	    else \
                echo "*** You need a C# compiler installed to build MCS. (make sure mcs works from the command line)" 1>&2 ; \
                echo "*** Read INSTALL.txt for information on how to bootstrap a Mono installation." 1>&2 ; \
	        exit 1; fi; fi

$(PROFILE_CS): $(topdir)/build/profiles/basic.make
	echo 'public class X { public static void Main () { System.Console.WriteLine ("OK"); } }' > $@

$(PROFILE_EXE): $(PROFILE_CS)
	$(EXTERNAL_MCS) /out:$@ $<

$(PROFILE_OUT): $(PROFILE_EXE)
	$(EXTERNAL_RUNTIME) $< > $@ 2>&1
	set x `wc -l $@`; case $$2 in 1) :;; *) exit 1;; esac
	grep '^OK$$' $@ > /dev/null
