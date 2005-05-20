thisdir = class/Microsoft.VisualBasic/Test/standalone

include ../../../../build/rules.make

DISTFILES = $(wildcard [0-9]*.vb) MainModule.vb

all-local install-local uninstall-local test-local run-test-on-dotnet-local:
	@:

clean-local:
	rm -f *.log *.exe *.mdb

run-test-local:
	fail=; logfile=results-$(PROFILE).log; rm -f $$logfile; \
	date > $$logfile ; \
	for i in [0-9]*.vb; do \
	  echo '*** ' $(BASCOMPILE) $$i MainModule.vb > $$i.log; \
	  if $(BASCOMPILE) $$i MainModule.vb >> $$i.log 2>&1 ; then \
	    exename=`echo $$i | sed s,.vb,.exe,` ; \
	    echo '*** $(TEST_RUNTIME) -O=-all' ./$$exename >> $$i.log; \
	    if $(TEST_RUNTIME) -O=-all ./$$exename >> $$i.log 2>&1; then \
	       rm -f $$exename $$i.log; \
	       echo "PASS: $$i" >> $$logfile; \
	       echo "PASS: $$i" ; \
	    else \
	       echo "Exit code: $$?" >> $$i.log ; \
	       echo "FAIL: $$i" >> $$logfile ; \
	       echo "FAIL: $$i" ; cat $$i.log ; \
	       fail="$$fail $$i" ; \
	    fi; \
	  else \
            echo "Exit code: $$?" >> $$i.log ; \
	    echo "FAIL: $$i: compilation" >> $$logfile ; \
	    echo "FAIL: $$i: compilation" ; cat $$i.log ; \
            fail="$$fail $$i" ; \
	  fi; \
	done; \
	test -z "$$fail" || { echo "Failing tests: $$fail"; exit 1; }

dist-local: dist-default
