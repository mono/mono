thisdir = class/Mono.Debugger.Soft
include ../../build/rules.make

LIBRARY = Mono.Debugger.Soft.dll
LIBRARY_SNK = ../mono.snk

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:Mono.Cecil.dll /r:System.Core.dll /unsafe -D:MONO_DATACONVERTER_STATIC_METHODS -keyfile:$(LIBRARY_SNK)

TEST_MCS_FLAGS = /r:Mono.Cecil.dll

test: dtest-app.exe dtest-excfilter.exe
check: dtest-app.exe dtest-excfilter.exe

dtest-app.exe: Test/dtest-app.cs
	$(CSCOMPILE) -out:$@ -unsafe -debug Test/dtest-app.cs

dtest-excfilter.exe: Test/dtest-excfilter.il
	MONO_PATH=$(topdir)/class/lib/$(PROFILE) $(INTERNAL_ILASM) -out:$@ /exe /debug Test/dtest-excfilter.il

CLEAN_FILES = dtest-app.exe dtest-app.exe.mdb dtest-excfilter.exe dtest-excfilter.exe.mdb

EXTRA_DISTFILES = \
	Test/dtest-app.cs \
	Test/dtest.cs \
	Test/dtest-excfilter.il

#NO_TEST = yes

ifneq (net_2_0, $(PROFILE))
NO_INSTALL = yes
endif

include ../../build/library.make
