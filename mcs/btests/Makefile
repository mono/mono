thisdir = btests
SUBDIRS = 
TEST_RUNTIME = mono
COMPILER_FLAGS = /libpath:../class/lib /imports:System
INTERNAL_MBAS = $(TEST_RUNTIME) --debug ../mbas/mbas.exe --stacktrace $(COMPILER_FLAGS)
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
	/bin/echo "********** Positive Tests ************" > results.out; \
	/bin/echo "********** Positive Tests ************" ; \
	for i in $(TEST_SOURCES) ; do \
		/bin/echo -n -e "========================\n$$i: " >> results.out; \
	    /bin/echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
            /bin/echo -n "COMPILED OK : " >> results.out; \
	        /bin/echo -n "COMPILED OK : " ;\
			true ; \
	    else \
			/bin/echo -n -e "FAILED COMPILATION\n---\n" >> results.out; \
			cat output >> results.out; \
	        /bin/echo FAILED COMPILATION ;  \
			continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe 1>output ; then \
			/bin/echo "EXECUTED OK" >> results.out; \
	        /bin/echo "EXECUTED OK" ; \
	    else \
			/bin/echo -n -e "$$i : FAILED AT RUNTIME\n---\n" >> results.out; \
			cat output >> results.out; \
	        /bin/echo "FAILED AT RUNTIME"; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done; \
	/bin/echo "========================" >> results.out

test-compiler-jit-ngtive_compile:
	@rm -f *.exe ; \
	/bin/echo "********** Negative Compilation Tests ************" >> results.out; \
	/bin/echo "********** Negative Compilation Tests ************" ; \
	for i in $(TEST_NGTIVE_COMPILATION_SOURCES) ; do \
		/bin/echo -n -e "========================\n$$i: " >> results.out; \
		/bin/echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb > /dev/null ; then \
			/bin/echo "UNEXPECTED BEHAVIOUR" >> results.out; \
	        /bin/echo UNEXPECTED BEHAVIOUR ; \
	    else \
			/bin/echo OK >> results.out; \
			/bin/echo OK; \
	        continue;\
	    fi ; \
	    rm -f ./$$i.exe output; \
	done

test-compiler-jit-ngtive_run:
	@rm -f *.exe; \
	/bin/echo "********** Negative Execution Tests ************" >> results.out; \
	/bin/echo "********** Negative Execution Tests ************" ; \
	for i in $(TEST_NGTIVE_RUNTIME_SOURCES) ; do \
	    /bin/echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
			true ; \
	    else \
			/bin/echo -n -e "\n$$i : FAILED COMPILATION\n " >> results.out; \
			cat output >> results.out; \
			/bin/echo -n -e "\n========================" >> results.out; \
	        /bin/echo FAILED COMPILATION ; \
	        continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe > /dev/null ; then \
			/bin/echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> results.out; \
			/bin/echo -n -e "\n========================" >> results.out; \
	        /bin/echo UNEXPECTED BEHAVIOUR ; \
	    else \
			/bin/echo OK >> results.out; \
			/bin/echo OK; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done
