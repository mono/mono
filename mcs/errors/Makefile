thisdir = errors
SUBDIRS = 
include ../build/rules.make

# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = $(INTERNAL_MCS)
LOCAL_MCS_FLAGS = /nowarn:162 --wlevel 4

GMCS_COMPILE = $(RUNTIME) ../gmcs/gmcs.exe $(LOCAL_MCS_FLAGS)
GENERICS_COMPILE = $(RUNTIME) ../gmcs/gmcs.exe $(LOCAL_MCS_FLAGS) /target:library

RUNTEST_PL = ./runtest.pl

DISTFILES = \
	CONTRIBUTORS_README 	\
	README.tests		\
	errors.txt 		\
	fail			\
	runtest.pl		\
	$(wildcard *.cs)

#all-local: run-test-local test-multi-local
all-local: CS0618-2-lib.dll CS0619-8-lib.dll CS0619-17-lib.dll CS0619-32-lib.dll CS0619-33-lib.dll CS0619-36-lib.dll \
CS3005-16-lib.dll CS3013-module.dll

test-local:

# again, run-test is when the tests actually happen, so
# don't compile on make test.

run-test-local: run-mcs-tests run-gmcs-tests run-generics-tests

run-mcs-tests:
	@ ./do-tests.pl mcs "$(CSCOMPILE)" "cs*.cs"

run-gmcs-tests:
	@ ./do-tests.pl gmcs "$(GMCS_COMPILE)" "cs*.cs"

run-generics-tests:
	@ ./do-tests.pl generics "$(GENERICS_COMPILE)" "gcs*.cs"

test-multi-local:
	@ failed=false; \
	testsuite_log=test-multi.log; rm -f $$testsuite_log ; \
	for i in error-*.cs; do \
		test_out=`echo $$i | sed 's,.cs$$,.out,'` ; \
		test_log=`echo $$i | sed 's,.cs$$,.log,'` ; \
		echo -n "Running test $$i ... "; \
		$(INTERNAL_MCS) $$i > $$test_out 2>&1 || : ; \
		if $(RUNTEST_PL) $$i $$test_out > $$test_log 2>&1 ; \
		then echo OK; rm -f $$test_out $$test_log ; \
		     echo "PASS: $$i" >> $$testsuite_log ; \
		else echo FAILED; \
		     echo "FAIL: $$i" >> $$testsuite_log ; \
		     flist="$$flist $$i"; \
		     failed=true; \
		fi; \
	done; \
	if $$failed; then \
		echo "The following tests failed: $$flist"; \
		exit 1; \
	else \
		echo All tests passed; \
	fi

clean-local:
	rm -f *.exe
	rm -f *.dll
	rm -f *.log

dist-local: dist-default

install-local uninstall-local:

%-lib.dll: %-lib.cs
	$(BOOTSTRAP_MCS) /target:library /out:$@ $<

%-module.dll: %-module.cs
	$(BOOTSTRAP_MCS) /target:module /out:$@ $<
