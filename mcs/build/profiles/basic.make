# -*- makefile -*-

BOOTSTRAP_MCS = $(EXTERNAL_MCS)
MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE):$$MONO_PATH" $(INTERNAL_MCS)

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -d:BOOTSTRAP_WITH_OLDLIB
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes

profile-check:
	@:

ifeq (.,$(thisdir))
all-recursive: real-profile-check
endif

real-profile-check:
	-rm -f basic-profile-check.cs basic-profile-check.exe
	@ok=:; \
	$$ok && echo 'class X { static void Main () { System.Console.Write("OK");}}' > basic-profile-check.cs; \
	$$ok && { $(EXTERNAL_MCS) basic-profile-check.cs || ok=false; }; \
	$$ok && { test -f basic-profile-check.exe || ok=false; }; \
	$$ok && { $(EXTERNAL_RUNTIME) ./basic-profile-check.exe > /dev/null || ok=false; }; \
	rm -f basic-profile-check.cs basic-profile-check.exe; \
	if $$ok; then :; else \
	    echo "*** The compiler '$(EXTERNAL_MCS)' doesn't appear to be usable." 1>&2 ; \
	    if test -f $(topdir)/class/lib/basic.tar.gz; then \
	        echo "*** Falling back to using pre-compiled binaries.  Be warned, this may not work." 1>&2 ; \
	        ( cd $(topdir)/class/lib; gzip -dc basic.tar.gz | tar xvf - ); \
		( cd $(topdir)/jay && $(MAKE) ); ( cd $(topdir)/mcs && $(MAKE) PROFILE=basic cs-parser.cs ); \
	        touch $(topdir)/class/lib/basic/*; \
	    else \
                echo "*** You need a C# compiler installed to build MCS. (make sure mcs works from the command line)" 1>&2 ; \
                echo "*** Read INSTALL.txt for information on how to bootstrap a Mono installation." 1>&2 ; \
	        exit 1; fi; fi

install-local: no-install
no-install:
	exit 1
