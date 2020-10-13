thisdir = class/Mono.Debugger.Soft
include ../../build/rules.make

LIBRARY = Mono.Debugger.Soft.dll
LIBRARY_SNK = ../mono.snk

LIB_REFS = System Mono.Cecil System.Core
LIB_MCS_FLAGS = /unsafe -D:MONO_DATACONVERTER_STATIC_METHODS -D:ENABLE_CECIL /publicsign
KEYFILE = $(LIBRARY_SNK)

ifndef MCS_MODE
DTEST_APP_FLAGS = -langversion:preview
endif

TEST_LIB_REFS = Mono.Cecil System System.Core

ifneq ($(filter monodroid monotouch monotouch_tv monotouch_watch wasm net_4_x,$(PROFILE)),)
test-local: build-dtest
endif

ifneq ($(filter monodroid monotouch monotouch_tv monotouch_watch wasm,$(PROFILE)),)
NO_INSTALL=1
NO_BUILD=1
NO_TEST=1
endif

test_output_dir=$(topdir)/class/lib/$(PROFILE)/tests

$(test_output_dir):
	mkdir -p $@

build-dtest: $(test_output_dir)/dtest-app.exe $(test_output_dir)/dtest-excfilter.exe

$(test_output_dir)/dtest-excfilter.exe: Test/dtest-excfilter.il | $(test_output_dir)
	$(ILASM) -out:$@ /exe /debug Test/dtest-excfilter.il

$(test_output_dir)/dtest-app.exe: Test/dtest-app.cs $(TEST_HELPERS_SOURCES) | $(test_output_dir)
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Core.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Runtime.CompilerServices.Unsafe.dll -sourcelink:Test/sourcelink.json -out:$@ -unsafe $(PLATFORM_DEBUG_FLAGS) $(DTEST_APP_FLAGS) -optimize- Test/dtest-app.cs $(TEST_HELPERS_SOURCES)

TEST_HELPERS_SOURCES = \
	../test-helpers/NetworkHelpers.cs \
	Test/TypeLoadClass.cs

EXTRA_DISTFILES = \
	Test/dtest-app.cs \
	Test/dtest.cs \
	Test/dtest-excfilter.il \
	Test/sourcelink.json \
	$(TEST_HELPERS_SOURCES)

CLEAN_FILES = $(addprefix $(test_output_dir)/, dtest-app.exe dtest-app.exe.mdb dtest-app.pdb dtest-excfilter.exe dtest-excfilter.exe.mdb dtest-excfilter.pdb)

include ../../build/library.make
