ifndef COMPILER
COMPILER = $(BASCOMPILE)
endif

ifndef PATTERN
PATTERN = *.vb
endif

COMPILER_FLAGS = /libpath:../../../class/lib/default /imports:System
LIBRARY_OPT = /target:library
DISTFILES = $(wildcard README.tests) $(wildcard *.vb)

run-test-local: 
	$(MAKE) clean-local
	../test-mbas.pl --compiler='$(COMPILER)' --compilerflags='$(COMPILER_FLAGS)' --pattern='$(PATTERN)' --runtime='$(TEST_RUNTIME)'

run-test-ondotnet-local:
	$(MAKE) clean-local
	../test-mbas.pl --compiler='$(COMPILER)' --compilerflags='$(COMPILER_FLAGS)' --pattern='$(PATTERN)' --runtime=

clean-local:
	rm -f *.exe *.log *.results

all-local test-local install-local uninstall-local:
	@:

dist-local: dist-default
