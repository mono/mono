thisdir = tests
SUBDIRS = 

ifndef COMPILER
COMPILER = mbas
endif

ifndef PATTERN
PATTERN = *.vb
endif

COMPILER_FLAGS = /libpath:../../../class/lib/default /imports:System
LIBRARY_OPT = /target:library
DISTFILES = README.tests $(wildcard *.vb)

all: run-test-local

run-test-local: 
	rm -f *.exe *.log *.results; \
	../test-mbas.pl --compiler=$(COMPILER) --compilerflags=$(COMPILER_FLAGS) --pattern=$(PATTERN)

all-local install-local test-local:
	@:





