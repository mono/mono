thisdir = @thisdir@
SUBDIRS = 
include ../../build/rules.make

ifeq (default, $(PROFILE))
# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)
endif

all-local $(STD_TARGETS:=-local):

%.res:
	@f=$*.cs; rm -f $$f; ln -s ../$$f $$f; \
	options=`sed -n 's,^// Compiler options:,,p' $$f`; \
	testlogfile="$*.log" ; \
        echo "*** $(CSCOMPILE) $$options $$f" > $$testlogfile ; \
	if $(CSCOMPILE) $$options $$f >> $$testlogfile 2>&1 ; then \
          if test -f $*.exe; then \
	    echo "*** $(TEST_RUNTIME) ./$*.exe" >> $$testlogfile ; \
	      if $(TEST_RUNTIME) -O=-all ./$*.exe >> $$testlogfile 2>&1 ; then \
		echo "PASS: $*" > $@ ; \
	        rm -f $$testlogfile ; \
	      else \
		echo "Exit code: $$?" >> $$testlogfile ; \
		echo "FAIL: $*" > $@ ; \
              fi ; \
	    else \
	      echo "PASS: $*: compilation" > $@ ; \
	      rm -f $$testlogfile ; \
	    fi ; \
	else \
	  echo "Exit code: $$?" >> $$testlogfile ; \
	  echo "FAIL: $*: compilation" > $@ ; \
	fi ; \
	rm -f $$f; \
	cat $@; \
	if test ! -f $$testlogfile ; then :; else cat $$testlogfile; fi

# test ordering
mtest-1-exe.res: mtest-1-dll.res
prog-1.res: dll-1.res
prog-2.res: dll-2.res
conv-main.res: conv-lib.res
vararg-exe.res: vararg-lib.res
module-2.res: module-1.res
module-3.res: module-1.res module-2.res
ns.res: ns0.res
gen-13-exe.res: gen-13-dll.res
gen-17-exe.res: gen-17-dll.res
gen-47-exe.res: gen-47-dll.res
