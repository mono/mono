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

run-test: 
	@ rm -f *.exe *.log *.results; 
	@ ../test-mbas.pl --compiler=$(COMPILER) --compilerflags=$(COMPILER_FLAGS) --pattern=$(PATTERN) --runtime=mono

run-test-ondotnet: 
	@ rm -f *.exe *.log *.results; 
	@ ../test-mbas.pl --compiler=$(COMPILER) --compilerflags=$(COMPILER_FLAGS) --pattern=$(PATTERN) --runtime=dotnet

all test clean install uninstall:
	@:





