MCS = mono $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target library --noconfig

all: .makefrag $(LIBRARY)

clean:
	-rm -rf $(LIBRARY) .response .makefrag library-deps.stamp

.response: $(LIB_LIST)
	cat $^ |egrep '\.cs$$' >$@

.makefrag: $(LIB_LIST) ../library.make
	echo -n "library-deps.stamp: " >$@.new
	cat $^ |egrep '\.cs$$' | sed -e 's,\.cs,.cs \\,' >>$@.new
	cat $@.new |sed -e '$$s, \\$$,,' >$@
	echo -e "\ttouch library-deps.stamp" >>$@
	rm -rf $@.new

-include .makefrag

$(LIBRARY): .response library-deps.stamp
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) $(MCS) $(MCS_FLAGS) -o $(LIBRARY) $(LIB_FLAGS) @.response
