thisdir = btests
SUBDIRS = 
COMPILER_FLAGS = /libpath:../class/lib /import:System
INTERNAL_MBAS = $(TEST_RUNTIME) ../mbas/mbas.exe --stacktrace $(COMPILER_FLAGS)
#INTERNAL_MBAS = vbc $(COMPILER_FLAGS)
all: test-compiler-jit 

include ../build/rules.make

DISTFILES = README.tests $(wildcard *.vb)

# We don't want debugging info :-)
USE_MCS_FLAGS :=

# include the test cases source file
include Test.Sources

# A test is a 'no pass' if it fails on either windows or linux

#TEST_NOPASS = \

all-local install-local:

# compilation is part of the testing too, so we don't actually 
# do anything in test-local.

test-local: 


run-test-local: test-compiler-jit 


test-compiler-jit: test-compiler-jit-real test-compiler-jit-ngtive_compile \
test-compiler-jit-ngtive_run


test-compiler-jit-real:
	@rm -f *.exe *.out ; \
	/usr/bin/echo "********** Positive Tests ************" > results.out; \
	/usr/bin/echo "********** Positive Tests ************" ; \
	for i in $(TEST_SOURCES) ; do \
		/usr/bin/echo -n -e "========================\n$$i: " >> results.out; \
	    /usr/bin/echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
            /usr/bin/echo -n "COMPILED OK : " >> results.out; \
	        /usr/bin/echo -n "COMPILED OK : " ;\
			true ; \
	    else \
			/usr/bin/echo -n -e "FAILED COMPILATION\n---\n" >> results.out; \
			cat output >> results.out; \
	        /usr/bin/echo FAILED COMPILATION ;  \
			continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe 1>output ; then \
			/usr/bin/echo "EXECUTED OK" >> results.out; \
	        /usr/bin/echo "EXECUTED OK" ; \
	    else \
			/usr/bin/echo -n -e "$$i : FAILED AT RUNTIME\n---\n" >> results.out; \
			cat output >> results.out; \
	        /usr/bin/echo "FAILED AT RUNTIME"; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done; \
	/usr/bin/echo "========================" >> results.out

test-compiler-jit-ngtive_compile:
	@rm -f *.exe ; \
	/usr/bin/echo "********** Negative Compilation Tests ************" >> results.out; \
	/usr/bin/echo "********** Negative Compilation Tests ************" ; \
	for i in $(TEST_NGTIVE_COMPILATION_SOURCES) ; do \
		/usr/bin/echo -n -e "========================\n$$i: " >> results.out; \
		/usr/bin/echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb > /dev/null ; then \
			/usr/bin/echo "UNEXPECTED BEHAVIOUR" >> results.out; \
	        /usr/bin/echo UNEXPECTED BEHAVIOUR ; \
	    else \
			/usr/bin/echo OK >> results.out; \
			/usr/bin/echo OK; \
	        continue;\
	    fi ; \
	    rm -f ./$$i.exe output; \
	done

test-compiler-jit-ngtive_run:
	@rm -f *.exe; \
	/usr/bin/echo "********** Negative Execution Tests ************" >> results.out; \
	/usr/bin/echo "********** Negative Execution Tests ************" ; \
	for i in $(TEST_NGTIVE_RUNTIME_SOURCES) ; do \
	    /usr/bin/echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
			true ; \
	    else \
			/usr/bin/echo -n -e "\n$$i : FAILED COMPILATION\n " >> results.out; \
			cat output >> results.out; \
			/usr/bin/echo -n -e "\n========================" >> results.out; \
	        /usr/bin/echo FAILED COMPILATION ; \
	        continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe > /dev/null ; then \
			/usr/bin/echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> results.out; \
			/usr/bin/echo -n -e "\n========================" >> results.out; \
	        /usr/bin/echo UNEXPECTED BEHAVIOUR ; \
	    else \
			/usr/bin/echo OK >> results.out; \
			/usr/bin/echo OK; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done
