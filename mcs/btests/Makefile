thisdir = btests
SUBDIRS = 
INTERNAL_MBAS = $(TEST_RUNTIME) --debug ../mbas/mbas.exe --stacktrace  /libpath:../class/lib

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
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(TEST_SOURCES) ; do \
		echo -n -e "========================\n$$i: " >> results.out; \
	    echo -n "$$i: "; \
	    if $(INTERNAL_MBAS) $$i.vb 1>output ; then \
            echo -n -e "COMPILED OK\n" >> results.out; \
	        echo -n "COMPILED OK : " ;\
			true ; \
	    else \
			echo -n -e "FAILED COMPILATION\n---\n" >> results.out; \
			cat output >> results.out; \
	        echo FAILED COMPILATION ;  \
			continue;\
	    fi ; \
		echo -n -e "------------------------\n" >> results.out; \
	    if $(TEST_RUNTIME) ./$$i.exe 1>output ; then \
			echo -n -e "$$i : EXECUTED OK\n" >> results.out; \
	        echo "EXECUTED OK" ; \
	    else \
			echo -n -e "$$i : FAILED AT RUNTIME\n---\n" >> results.out; \
			cat output >> results.out; \
	        echo "FAILED AT RUNTIME"; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done; \
	echo -n -e "========================\n$$i: " >> results.out

test-compiler-jit-ngtive_compile:
	@rm -f *.exe \
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(TEST_NGTIVE_COMPILATION_SOURCES) ; do \
	    echo -n "$$i: "; \
	     if $(INTERNAL_MBAS) $$i.vb > /dev/null ; then \
		echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> results.out; \
		echo -n -e "\n========================" >> results.out; \
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
		echo -n -e "\n$$i : FAILED COMPILATION\n " >> results.out; \
		cat output >> results.out; \
		echo -n -e "\n========================" >> results.out; \
	        echo FAILED COMPILATION ;  continue;\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe > /dev/null ; then \
		echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> results.out; \
		echo -n -e "\n========================" >> results.out; \
	        echo UNEXPECTED BEHAVIOUR ; \
	    else \
		echo OK; \
	    fi ; \
	    rm -f ./$$i.exe output; \
	done





#GENERIC_KNOWN_FAILURES = test-55


