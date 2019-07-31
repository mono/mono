thisdir = class/Mono.Profiler.Log
include ../../build/rules.make

LIBRARY = Mono.Profiler.Log.dll
LIBRARY_WARN_AS_ERROR = yes

KEYFILE = $(LIBRARY_SNK)

LIB_REFS = System System.Core
LIB_MCS_FLAGS = /unsafe /publicsign /nowarn:0618 -D:MONO_DATACONVERTER_STATIC_METHOD

ifeq (net_4_x,$(PROFILE))

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks

xunit-test-local: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/tests/log-profiler-test.exe

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tests/log-profiler-test.exe: Test/log-profiler-test.cs
	mkdir -p $(dir $@)
	$(CSCOMPILE) $(PLATFORM_DEBUG_FLAGS) /unsafe $(if $(MCS_MODE),,/warnaserror) /r:$(build_libdir)/mscorlib.dll /r:$(build_lib) /out:$@ $<

CLEAN_FILES = \
	$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tests/log-profiler-test.exe \
	$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tests/log-profiler-test.exe.mdb \
	$(topdir)/class/lib/$(PROFILE_DIRECTORY)/tests/log-profiler-test.pdb

else

NO_TEST = yes

endif

EXTRA_DISTFILES = \
	Test/log-profiler-test.cs

include ../../build/library.make
