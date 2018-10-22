thisdir = class/Mono.Debugger.Soft
include ../../build/rules.make

LIBRARY = Mono.Debugger.Soft.dll
LIBRARY_SNK = ../mono.snk

LIB_REFS = System Mono.Cecil System.Core
LIB_MCS_FLAGS = /unsafe -D:MONO_DATACONVERTER_STATIC_METHODS /publicsign
KEYFILE = $(LIBRARY_SNK)

TEST_LIB_REFS = Mono.Cecil System System.Core

VALID_TEST_PROFILE := $(filter net_4_x, $(PROFILE))

ifdef VALID_TEST_PROFILE

test-local: build-dtest

else

NO_TEST=1
check:

endif

test_output_dir=$(topdir)/class/lib/$(PROFILE)/tests

$(test_output_dir):
	mkdir -p $@

build-dtest: $(test_output_dir)/dtest-app.exe $(test_output_dir)/dtest-excfilter.exe

$(test_output_dir)/dtest-excfilter.exe: Test/dtest-excfilter.il | $(test_output_dir)
	$(ILASM) -out:$@ /exe /debug Test/dtest-excfilter.il

$(test_output_dir)/dtest-app.exe: Test/dtest-app.cs $(TEST_HELPERS_SOURCES) | $(test_output_dir)
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Core.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll -sourcelink:Test/sourcelink.json -out:$@ -unsafe $(PLATFORM_DEBUG_FLAGS) -optimize- Test/dtest-app.cs $(TEST_HELPERS_SOURCES)

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
