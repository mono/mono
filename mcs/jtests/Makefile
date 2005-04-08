thisdir = jtests
SUBDIRS = 
INTERNAL_MJS = $(TEST_RUNTIME) $(topdir)/tools/mjs/mjs.exe
all: test-compiler-jit
include ../build/rules.make
include Test.Sources

all-local install-local:

test-local:

run-test-local: test-compiler-jit

test-compiler-jit: test-compiler-jit-real

test-compiler-jit-real:
	@rm -f *.exe *.out; \
	/bin/echo "*** Tests ***" > results.out; \
	/bin/echo "*** Tests ***"; \
	for i in $(TEST_SOURCES) ; do \
		/bin/echo -n -e "===\n$$i: " >> results.out; \
	    /bin/echo -n "$$i: "; \
	    if $(INTERNAL_MJS) $$i.js 1>output ; then \
	    /bin/echo -n "COMPILED OK : " >> results.out; \
		/bin/echo -n "COMPILED OK : " ; \
		    true; \
	   else \
			/bin/echo -n -e "FAILED COMPILATION\n---\n" >> results.out; \
			cat output >> results.out; \
	      /bin/echo "FAILED COMPILATION" ; \
		     continue; \
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe 1>output ; then \
			/bin/echo "EXECUTED OK" >> results.out; \
		/bin/echo "EXECUTED OK"; \
	    else \
			/bin/echo -n -e "$$i : FAILED AT RUNTIME\n---\n" >> results.out; \
			cat output >> results.out; \
		/bin/echo "$$i : FAILED AT RUNTIME" ; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done; \
	/bin/echo "===" >> results.out

clean-local:
	rm *~

