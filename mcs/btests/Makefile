thisdir = btests
SUBDIRS = 
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
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(TEST_SOURCES) ; do \
	    echo -n "$$i: "; \
	     if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
		true ; \
	    else \
		echo -n -e "\n$$i : FAILED COMPILATION\n " >> failures.out; \
		cat output >> failures.out; \
		echo -n -e "\n========================" >> failures.out; \
	        echo FAILED COMPILATION ;  continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe 1>output ; then \
	        echo OK ; \
	    else \
		echo -n -e "\n$$i : FAILED AT RUNTIME\n " >> failures.out; \
		cat output >> failures.out; \
		echo -n -e "\n========================" >> failures.out; \
	        echo FAILED ; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done

test-compiler-jit-ngtive_compile:
	@rm -f *.exe \
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(TEST_NGTIVE_COMPILATION_SOURCES) ; do \
	    echo -n "$$i: "; \
	     if $(INTERNAL_MBAS) $$i.vb > /dev/null ; then \
		echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> failures.out; \
		echo -n -e "\n========================" >> failures.out; \
	        echo UNEXPECTED BEHAVIOUR ; \
	    else \
		echo OK; \
	        continue;\
	    fi ; \
	    rm -f ./$$i.exe output; \
	done


test-compiler-jit-ngtive_run:
	@rm -f *.exe \
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(TEST_NGTIVE_RUNTIME_SOURCES) ; do \
	    echo -n "$$i: "; \
	     if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
		true ; \
	    else \
		echo -n -e "\n$$i : FAILED COMPILATION\n " >> failures.out; \
		cat output >> failures.out; \
		echo -n -e "\n========================" >> failures.out; \
	        echo FAILED COMPILATION ;  continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe > /dev/null ; then \
		echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> failures.out; \
		echo -n -e "\n========================" >> failures.out; \
	        echo UNEXPECTED BEHAVIOUR ; \
	    else \
		echo OK; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done





#GENERIC_KNOWN_FAILURES = test-55


