thisdir = errors
SUBDIRS = 
include ../build/rules.make

# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = $(INTERNAL_MCS)
LOCAL_MCS_FLAGS = /nowarn:162 --wlevel 4

GMCS_COMPILE = $(RUNTIME) ../gmcs/gmcs.exe $(MCS_FLAGS)

RUNTEST_PL = ./runtest.pl

DISTFILES = \
	CONTRIBUTORS_README 	\
	README.tests		\
	errors.txt 		\
	fail			\
	runtest.pl		\
	$(wildcard *.cs)

#all-local: run-test-local test-multi-local
all-local: CS0619-8-lib.dll CS0619-17-lib.dll CS3005-16-lib.dll CS3013-module.dll

test-local:

# again, run-test is when the tests actually happen, so
# don't compile on make test.

run-test-local:
	@ ./do-tests.pl mcs "$(CSCOMPILE)" "cs*.cs"

run-generics-local:
	@ ./do-tests.pl gmcs "$(GMCS_COMPILE)" "gcs*.cs"

test-multi-local:
	@ failed=false; \
	for i in error-*.cs; do \
		echo -n "Running test $$i ... "; \
		if MCS='$(INTERNAL_MCS)' $(RUNTEST_PL) $$i > /dev/null; \
		then echo OK; \
		else echo FAILED; \
		     flist="$$flist $$i"; \
		     failed=true; \
		fi; \
	done; \
	if $$failed; then \
		echo "The following tests failed: $$flist"; \
	else \
		echo All tests passed; \
	fi

clean-local:
	rm -f *.exe
	rm -f *.dll

dist-local: dist-default

install-local uninstall-local:

CS0619-8-lib.dll:
	$(BOOTSTRAP_MCS) /target:library /out:$@ CS0619-8-lib.cs

CS0619-17-lib.dll:
	$(BOOTSTRAP_MCS) /target:library /out:$@ CS0619-17-lib.cs

CS3005-16-lib.dll: 
	$(BOOTSTRAP_MCS) /target:library /out:$@ CS3005-16-lib.cs

CS3013-module.dll:
	$(BOOTSTRAP_MCS) /target:module /out:$@ CS3013-module.cs
