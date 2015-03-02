thisdir = class/Mono.Debugger.Soft
include ../../build/rules.make

LIBRARY = Mono.Debugger.Soft.dll
LIBRARY_SNK = ../mono.snk

LIB_REFS = System Mono.Cecil System.Core
LIB_MCS_FLAGS = /r:$(corlib) /unsafe -D:MONO_DATACONVERTER_STATIC_METHODS -keyfile:$(LIBRARY_SNK)

TEST_MCS_FLAGS = /r:Mono.Cecil.dll /r:System.dll /r:System.Core.dll

VALID_TEST_PROFILE := $(filter net_4_5, $(PROFILE))

# The test exe is not profile specific, and compiling a 2.0 will make the 4.5 tests fail
ifdef VALID_TEST_PROFILE

test-local: dtest-app.exe dtest-excfilter.exe

dtest-app.exe: Test/dtest-app.cs
	$(CSCOMPILE) -out:$@ -unsafe -debug -optimize- Test/dtest-app.cs

dtest-excfilter.exe: Test/dtest-excfilter.il
	MONO_PATH=$(topdir)/class/lib/$(PROFILE) $(INTERNAL_ILASM) -out:$@ /exe /debug Test/dtest-excfilter.il

else

NO_TEST=1
check:

endif

CLEAN_FILES = dtest-app.exe dtest-app.exe.mdb dtest-excfilter.exe dtest-excfilter.exe.mdb

EXTRA_DISTFILES = \
	Test/dtest-app.cs \
	Test/dtest.cs \
	Test/dtest-excfilter.il

#NO_TEST = yes

include ../../build/library.make
