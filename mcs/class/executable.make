RUNTIME = mono
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target exe
INSTALL = /usr/bin/install
prefix = /usr

all: $(PROGRAM)

clean:
	-rm -rf $(PROGRAM) .response-exe .makefrag-exe

.response-exe: $(PROGRAM_LIST)
	cat $^ |egrep '\.cs$$' >$@

.makefrag-exe: $(PROGRAM_LIST)
	echo -n "program-deps: " >$@.new
	cat $^ |egrep '\.cs$$' | sed -e 's,\.cs,.cs \\,' >>$@.new
	cat $@.new |sed -e '$$s, \\$$,,' >$@
	rm -rf $@.new

-include .makefrag-exe

$(PROGRAM): .response-exe .makefrag-exe program-deps
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) $(MCS) $(MCS_FLAGS) -o $(PROGRAM) $(PROGRAM_FLAGS) @.response-exe
	touch -r $(PROGRAM) program-deps

install: all
	mkdir -p $(prefix)/bin/
	$(INSTALL) -m 755 $(PROGRAM) $(prefix)/bin/

