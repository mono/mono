thisdir = errors
SUBDIRS = 
include ../build/rules.make

ifeq (default, $(PROFILE))
# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)
endif

GENERICS_COMPILE = $(CSCOMPILE) /target:library

DISTFILES = \
	CONTRIBUTORS_README 	\
	README.tests		\
	errors.txt 		\
	do-tests.pl		\
	$(wildcard *.cs)	\
	$(wildcard *.inc)	\
	$(wildcard *.pub)	\
	$(wildcard *.response)	\
	mcs-expect-no-error mcs-expect-wrong-error mcs-ignore-tests \
	gmcs-expect-no-error gmcs-expect-wrong-error gmcs-ignore-tests \
	generics-expect-no-error generics-expect-wrong-error \
	TestRunner.exe know-issues-*

all-local: CS0118-2-lib.dll CS0122-10-lib.dll CS0534-3-lib.dll CS0534-4-lib.dll CS0571-3-lib.dll \
	CS0612-2-lib.dll CS0618-2-lib.dll CS0619-8-lib.dll CS0619-17-lib.dll CS0619-32-lib.dll CS0619-33-lib.dll CS0619-36-lib.dll \
	CS3005-16-lib.dll CS3013-module.dll

test-local:

run-test-ondotnet-local:

# again, run-test is when the tests actually happen, so
# don't compile on make test.

run-test-local: run-mcs-tests 

test-everything:
	$(MAKE) PROFILE=default run-test
	$(MAKE) PROFILE=net_2_0 run-test

run-mcs-tests: TestRunner.exe
ifeq (net_2_0, $(PROFILE))
	-rm -f generics.log
	@./do-tests.pl generics '$(GENERICS_COMPILE)' "gcs*.cs"

#	Is disabled until we solve problem with running
#	$(RUNTIME) TestRunner.exe "2" $(topdir)/class/lib/$(PROFILE)/gmcs.exe know-issues-gmcs
else
	$(RUNTIME) TestRunner.exe "1" $(topdir)/class/lib/$(PROFILE)/mcs.exe know-issues-mcs
endif

clean-local:
	rm -f *.exe *.dll *.log *.mdb dummy.xml *.junk

dist-local: dist-default

install-local uninstall-local:

%-lib.dll: %-lib.cs
	$(BOOTSTRAP_MCS) /target:library /out:$@ $<

%-module.dll: %-module.cs
	$(BOOTSTRAP_MCS) /target:module /out:$@ $<

TestRunner.exe:
	$(CSCOMPILE) TestRunner.cs