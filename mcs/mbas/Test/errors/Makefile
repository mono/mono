thisdir = errors
SUBDIRS = 
TEST_RUNTIME = mono
COMPILER_FLAGS = /libpath:../class/lib/default/ /imports:System
INTERNAL_MBAS = $(TEST_RUNTIME) --debug ../mbas/mbas.exe --stacktrace $(COMPILER_FLAGS)
LIBRARY_OPT = /target:library
DISTFILES = README.tests $(wildcard *.vb)

all: run-test-local

run-test-local: 
	rm -f *.exe *.log *.results; \
	../test-mbas.pl

all-local install-local test-local:
	@:





