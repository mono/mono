thisdir = btests
SUBDIRS = 
TEST_RUNTIME = mono
COMPILER_FLAGS = /libpath:../class/lib/default/ /imports:System
INTERNAL_MBAS = $(TEST_RUNTIME) --debug ../mbas/mbas.exe --stacktrace $(COMPILER_FLAGS)
LIBRARY_OPT = /target:library
SUPPORT_DLLS = /r:./Dll1.dll,./Override_dll.dll,./Delegate_dll.dll,./Event_dll.dll,./OptionalArgu_dll.dll
#INTERNAL_MBAS = vbc $(COMPILER_FLAGS)
all: test-compiler-jit 

# removed this cause INTERNAL_MBAS now getting set in 
# rules.make which is conflicting with this instance of 
# INTERNAL_MBAS
#include ../build/rules.make

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
#  echo "Inside loop" ; \
#  echo $$i | sed -e 's/\*//' | >> $$target; \ 
#  echo $$target; \

test-local: 


run-test-local: test-compiler-jit 


test-compiler-jit: test-compiler-jit-real test-compiler-jit-ngtive_compile \
test-compiler-jit-ngtive_run

TEST_TAG = mbas-positive

test-dlls:
	@logpath=./logs ; mkdir -p $$logpath ; \
	logfile="$$logpath/$(TEST_TAG).results"; \
	rm -f *.dll $$logpath/$(TEST_TAG).results $$logpath/$(TEST_TAG)-*.log ; \
	echo "********** Support DLLs ************" > $$logfile ; \
	echo "********** Support DLLs ************" ; \
	for i in $(TEST_DLL_SOURCES) ; do \
	    echo -n -e "========================\n$$i: " >> $$logfile; \
	    testlogfile="$$logpath/$(TEST_TAG)-$$i.log" ; \
	    echo -n "$$i: "; \
	    echo "*** $(INTERNAL_MBAS) $(LIBRARY_OPT) $$i.vb" > $$testlogfile ; \
	    if $(INTERNAL_MBAS) $(LIBRARY_OPT)  $$i.vb >> $$testlogfile 2>&1 ; then \
		echo "COMPILED OK  " ; \
	    else \
		echo "Exit code: $$?" >> $$testlogfile ; \
		failed="$$failed $$i" ; \
		echo "FAILED: $$i: COMPILATION" >> $$logfile; \
		echo FAILED COMPILATION ; \
		continue; \
	    fi ; \
	done ; 

test-compiler-jit-real: test-dlls
	@logpath=./logs ; mkdir -p $$logpath ; \
	logfile="$$logpath/$(TEST_TAG).results"; \
	rm -f *.exe ; \
	echo "********** Positive Tests ************" >> $$logfile ; \
	echo "********** Positive Tests ************" ; \
	for i in $(TEST_SOURCES) ; do \
	    targetexe=`echo "$$i" |sed -e 's,\*,,'` ; \
	    echo -n -e "========================\n$$i: " >> $$logfile; \
	    testlogfile="$$logpath/$(TEST_TAG)-$$targetexe.log" ; \
	    echo -n "$$i: "; \
	    echo "*** $(INTERNAL_MBAS) $(SUPPORT_DLLS) $$i.vb" > $$testlogfile ; \
	    if $(INTERNAL_MBAS) $(SUPPORT_DLLS) /o:$$targetexe.exe $$i.vb >> $$testlogfile 2>&1 ; then \
		echo -n "COMPILED OK : " ; \
	    else \
		echo "Exit code: $$?" >> $$testlogfile ; \
		failed="$$failed $$i" ; \
		echo "FAILED: $$i: COMPILATION" >> $$logfile; \
		echo FAILED COMPILATION ; \
		continue; \
	    fi ; \
	    echo "*** $(TEST_RUNTIME) ./$$targetexe.exe" >> $$testlogfile ; \
	    if $(TEST_RUNTIME) ./$$targetexe.exe >> $$testlogfile 2>&1 ; then \
		echo "EXECUTED OK: $$i" >> $$logfile ; \
	        echo "EXECUTED OK" ; rm -f $$testlogfile ; \
	    else \
		echo "$$i : FAILED AT RUNTIME Exit code: $$?" >> $$testlogfile ; \
		failed="$$failed $$i" ; \
		echo "FAILED AT RUNTIME"; \
	    fi ; \
	    rm -f ./$$targetexe.exe ; \
	done ; \

test-compiler-jit-ngtive_compile: TEST_TAG = mbas-negative-compile
test-compiler-jit-ngtive_compile: 
	@logpath=./logs ; mkdir -p $$logpath; \
	logfile="$$logpath/$(TEST_TAG).results"; \
	rm -f *.exe $$logpath/$(TEST_TAG).results $$logpath/$(TEST_TAG)-*.log ; \
	echo "********** Negative Compilation Tests ************" >> $$logfile; \
	echo "********** Negative Compilation Tests ************" ; \
	for i in $(TEST_NGTIVE_COMPILATION_SOURCES) ; do \
	    echo -n -e "========================\n$$i: " >> $$logfile; \
	    testlogfile="$$logpath/$(TEST_TAG)-$$i.log" ; \
	    echo -n "$$i: "; \
	    echo "*** $(INTERNAL_MBAS) $$i.vb" > $$testlogfile ; \
	    if $(INTERNAL_MBAS) $$i.vb >> $$testlogfile 2>&1 ; then \
		echo "UNEXPECTED BEHAVIOUR" >> $$logfile ; \
	        echo UNEXPECTED BEHAVIOUR ; \
	    else \
		echo "Exit code: $$?" >> $$testlogfile ; \
		echo "OK" >> $$logfile ; \
		echo OK; \
	    fi ; \
	    rm -f ./$$i.exe ; \
	done


test-compiler-jit-ngtive_run: TEST_TAG = mbas-negative-runtime
test-compiler-jit-ngtive_run:
	@logpath=./logs ; mkdir -p $$logpath; \
	logfile="$$logpath/$(TEST_TAG).results"; \
	rm -f *.exe $$logpath/$(TEST_TAG).results $$logpath/$(TEST_TAG)-*.log ; \
	echo "****** Negative Execution Tests ********" >> $$logfile; \
	echo "****** Negative Execution Tests ********" ; \
	for i in $(TEST_NGTIVE_RUNTIME_SOURCES) ; do \
	    testlogfile="$$logpath/$(TEST_TAG)-$$i.log" ; \
	    echo -n "$$i: "; \
	    echo "*** $(INTERNAL_MBAS) $$i.vb" > $$testlogfile ; \
	    if $(INTERNAL_MBAS) $$i.vb >> $$testlogfile 2>&1 ; then \
		true ; \
	    else \
		echo "Exit code: $$?" >> $$testlogfile ; \
		failed="$$failed $$i" ; \
		echo -n -e "\n$$i : FAILED COMPILATION\n " >> $$logfile; \
		echo FAILED COMPILATION ; \
	        continue; \
	    fi ; \
	    echo "*** $(TEST_RUNTIME) ./$$i.exe" >> $$testlogfile ; \
	    if $(TEST_RUNTIME) ./$$i.exe >> $$testlogfile 2>&1 ; then \
		echo -n -e "\n$$i : UNEXPECTED BEHAVIOUR\n " >> $$logfile; \
		echo -n -e "\n========================" >> $$logfile; \
	        echo UNEXPECTED BEHAVIOUR ; rm -f $$testlogfile ; \
	    else \
		echo OK >> $$logfile; \
		echo OK; \
	    fi ; \
	    rm -f ./$$i.exe ; \
	done

