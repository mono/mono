MCS = mcs
MCS_FLAGS = --target exe

all: $(PROGRAM)

clean-exe:
	-rm -rf $(PROGRAM) .response-exe .makefrag-exe

.response-exe: $(PROGRAM_LIST)
	cat $^ |egrep '\.cs$$' >$@

.makefrag-exe: $(PROGRAM_LIST)
	echo -n "program-deps: " >$@.new
	cat $^ |egrep '\.cs$$' | sed -e 's,\.cs,.cs \\,' >>$@.new
	cat $@.new |sed -e '$$s, \\$$,,' >$@
	rm -rf $@.new

-include .makefrag-exe

$(PROGRAM): .response-exe .makefrag-exe #program-deps
	$(MCS) $(MCS_FLAGS) -o $(PROGRAM) $(PROGRAM_FLAGS) @.response-exe
