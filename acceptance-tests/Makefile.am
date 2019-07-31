ACCEPTANCE_TESTS_PATH=external
BENCHMARKER_PATH=$(ACCEPTANCE_TESTS_PATH)/benchmarker
ROSLYN_PATH=$(ACCEPTANCE_TESTS_PATH)/roslyn
CORECLR_PATH=$(ACCEPTANCE_TESTS_PATH)/coreclr
MSTESTSUITE_PATH=$(ACCEPTANCE_TESTS_PATH)/ms-test-suite
DEBIANSHOOTOUTMONO_PATH=$(ACCEPTANCE_TESTS_PATH)/DebianShootoutMono

CLEANFILES = *.dll *.exe *.mdb
EXTRA_DIST=README.md SUBMODULES.json microbench.mk versions.mk profiler-stress.mk roslyn.mk coreclr.mk ms-test-suite.mk

CLASS=$(mcs_topdir)/class/lib/$(DEFAULT_PROFILE)
TOOLS_CLASS=$(mcs_topdir)/class/lib/build

RUNTIME = MONO_PATH=$(CLASS) $(abs_top_builddir)/runtime/mono-wrapper --debug
TOOLS_RUNTIME = MONO_PATH=$(TOOLS_CLASS) $(abs_top_builddir)/runtime/mono-wrapper --debug
MCS = $(TOOLS_RUNTIME) $(CSC) -nologo -noconfig -lib:$(CLASS) -r:System.dll -r:System.Core.dll -r:System.Xml.dll -r:Microsoft.CSharp.dll
ILASM = $(TOOLS_RUNTIME) $(TOOLS_CLASS)/ilasm.exe
XUNIT = $(RUNTIME) $(abs_top_builddir)/external/xunit-binaries/xunit.console.exe

include versions.mk
include microbench.mk
include profiler-stress.mk
include roslyn.mk
include coreclr.mk
include ms-test-suite.mk

check-full: check-profiler-stress check-roslyn check-coreclr check-ms-test-suite

clean-local: clean-local-coreclr
