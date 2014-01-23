thisdir = class/Mono.Debugger.Soft
include ../../build/rules.make

LIBRARY = Mono.Debugger.Soft.dll
LIBRARY_SNK = ../mono.snk

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:Mono.Cecil.dll /r:System.Core.dll /unsafe -D:MONO_DATACONVERTER_STATIC_METHODS -keyfile:$(LIBRARY_SNK)

TEST_MCS_FLAGS = /r:Mono.Cecil.dll /r:System.dll /r:System.Core.dll

test-local: dtest-app.exe dtest-excfilter.exe

dtest-app.exe: Test/dtest-app.cs
	$(CSCOMPILE) -out:$@ -unsafe -debug -optimize- Test/dtest-app.cs

dtest-excfilter.exe: Test/dtest-excfilter.il
	MONO_PATH=$(topdir)/class/lib/$(PROFILE) $(INTERNAL_ILASM) -out:$@ /exe /debug Test/dtest-excfilter.il

CLEAN_FILES = dtest-app.exe dtest-app.exe.mdb dtest-excfilter.exe dtest-excfilter.exe.mdb

EXTRA_DISTFILES = \
	Test/dtest-app.cs \
	Test/dtest.cs \
	Test/dtest-excfilter.il

#NO_TEST = yes

include ../../build/library.make
