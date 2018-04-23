thisdir = class/Mono.Profiler.Log
include ../../build/rules.make

LIBRARY = Mono.Profiler.Log.dll
LIBRARY_WARN_AS_ERROR = yes

KEYFILE = $(LIBRARY_SNK)

LIB_REFS = System System.Core
LIB_MCS_FLAGS = /unsafe /publicsign /nowarn:0618

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks

xunit-test-local: log-profiler-test.exe

log-profiler-test.exe: Test/log-profiler-test.cs
	$(CSCOMPILE) $(PLATFORM_DEBUG_FLAGS) /unsafe /warnaserror /r:$(build_libdir)/mscorlib.dll /r:$(build_lib) /out:$@ $<

EXTRA_DISTFILES = \
	Test/log-profiler-test.cs

CLEAN_FILES = \
	log-profiler-test.exe \
	log-profiler-test.exe.mdb \
	log-profiler-test.pdb

include ../../build/library.make
