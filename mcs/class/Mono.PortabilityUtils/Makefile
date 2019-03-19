thisdir = class/Mono.PortabilityUtils
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.PortabilityUtils.dll

LIB_REFS = System System.Windows.Forms
KEYFILE = ../mono.pub
LIB_MCS_FLAGS =

TEST_LIB_REFS = Mono.PortabilityUtils System System.Core System.Drawing System.Windows.Forms

include ../../build/library.make


test-local: winforms_test.exe

clean: clean-test-binary-files

winforms_test.exe: Test/Mono.PortabilityUtils/winforms_test.cs \
				   Test/Mono.PortabilityUtils/WinFormsTestHelpers.cs \
				   $(topdir)/class/lib/$(PROFILE)/Mono.PortabilityUtils.dll \
				   $(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll \
				   $(topdir)/class/lib/$(PROFILE)/System.Core.dll \
				   $(topdir)/class/lib/$(PROFILE)/System.dll \
				   $(topdir)/class/lib/$(PROFILE)/mscorlib.dll
	$(CSCOMPILE) -out:Test/$@ \
		Test/Mono.PortabilityUtils/winforms_test.cs \
		Test/Mono.PortabilityUtils/WinFormsTestHelpers.cs \
		-r:$(topdir)/class/lib/$(PROFILE)/Mono.PortabilityUtils.dll \
		-r:$(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll \
		-r:$(topdir)/class/lib/$(PROFILE)/System.Core.dll \
		-r:$(topdir)/class/lib/$(PROFILE)/System.dll \
		-r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

run-winforms_test.exe-from-test:
	# To run:
	#   make run-winforms_test.exe-from-test args="<command line argumens>"
	$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) Test/winforms_test.exe $(args)

clean-test-binary-files:
	rm -f Test/*.exe
	rm -f Test/*.pdb
