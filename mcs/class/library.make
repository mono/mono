MCS = mcs
MCS_FLAGS = --target library --noconfig

all: $(LIBRARY)

clean:
	-rm -rf $(LIBRARY) .response .makefrag

.response: $(LIB_LIST)
	cat $^ |egrep '\.cs$$' >$@

.makefrag: $(LIB_LIST)
	echo -n "library-deps: " >$@.new
	cat $^ |egrep '\.cs$$' | sed -e 's,\.cs,.cs \\,' >>$@.new
	cat $@.new |sed -e '$$s, \\$$,,' >$@
	rm -rf $@.new

-include .makefrag

$(LIBRARY): .response .makefrag #library-deps
	$(MCS) $(MCS_FLAGS) -o $(LIBRARY) $(LIB_FLAGS) @.response
