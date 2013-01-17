thisdir = tools/compiler-tester
SUBDIRS = 
include ../../build/rules.make

PROGRAM = compiler-tester.exe
NO_INSTALL = yes

ifeq (moonlight, $(PROFILE))

all-local: $(PROGRAM)
clean-local:
	rm -f $(PROGRAM)

$(PROGRAM): $(dir $(PROGRAM))/.stamp
$(PROGRAM): $(topdir)/class/lib/moonlight_raw/compiler-tester.exe
	cp $< $@

dist-local: dist-default
install-local uninstall-local test-local run-test-local run-test-ondotnet-local: ; @:

else

include $(topdir)/build/executable.make

endif