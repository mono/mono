thisdir = errors
SUBDIRS = 
include ../build/rules.make

# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = $(INTERNAL_MCS)
LOCAL_MCS_FLAGS = /nowarn:162 /unsafe --wlevel 4

RUNTEST_PL = ./runtest.pl


DISTFILES = \
	CONTRIBUTORS_README 	\
	README.tests		\
	errors.txt 		\
	fail			\
	runtest.pl		\
	$(wildcard *.cs)

all-local: run-test-local test-multi-local

# again, run-test is when the tests actually happen, so
# don't compile on make test.

run-test-local:
	@ failed=false; \
	for i in cs*.cs; do \
		case $$i in \
			cs[0-9]*-[0-9]*cs) \
			error=`echo $$i | sed -e 's/cs*//' -e 's/.cs//' -e 's/-.*//'`; \
			;;\
			cs[0-9-]*cs)  \
			error=`echo $$i | sed -e 's/cs*//' -e 's/.cs//'`; \
			;;\
		esac; \
		echo -n "Running test $$i ... "; \
		if $(CSCOMPILE) --expect-error $$error $$i > /dev/null; \
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

dist-local: dist-default
