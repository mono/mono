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
	@f=../$*.cs; options=`sed -n 's,^// Compiler options:,,p' $$f`; \
	case $$options in *-t:library*) ext=dll ;; *-t:module*) ext=netmodule ;; *) ext=exe ;; esac; \
	testlogfile="$*.log" ; \
        echo "*** $(CSCOMPILE) $$options -out:$*.$$ext $$f" > $$testlogfile ; \
	if $(CSCOMPILE) $$options -out:$*.$$ext $$f >> $$testlogfile 2>&1 ; then \
	  if test -f $*.exe; then \
	    echo '*** $(TEST_RUNTIME) ./$*.exe' >> $$testlogfile ; \
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
	cat $@; \
	if test ! -f $$testlogfile ; then :; else cat $$testlogfile; fi

# test ordering dependencies will be pasted after this
