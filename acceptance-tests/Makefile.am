ACCEPTANCE_TESTS_PATH=external
BENCHMARKER_PATH=$(ACCEPTANCE_TESTS_PATH)/benchmarker
ROSLYN_PATH=$(ACCEPTANCE_TESTS_PATH)/roslyn
CORECLR_PATH=$(ACCEPTANCE_TESTS_PATH)/coreclr
MSTESTSUITE_PATH=$(ACCEPTANCE_TESTS_PATH)/ms-test-suite

CLEANFILES = *.dll *.exe *.mdb
EXTRA_DIST=README.md SUBMODULES.json versions.mk roslyn.mk coreclr.mk ms-test-suite.mk

CLASS=$(mcs_topdir)/class/lib/$(DEFAULT_PROFILE)

with_mono_path = MONO_PATH=$(CLASS)
RUNTIME = $(with_mono_path) $(abs_top_builddir)/runtime/mono-wrapper --debug
MCS = $(RUNTIME) $(mcs_topdir)/class/lib/build/mcs.exe
ILASM = $(RUNTIME) $(CLASS)/ilasm.exe

include versions.mk
include profiler-stress.mk
include roslyn.mk
include coreclr.mk
include ms-test-suite.mk

check-full: check-profiler-stress check-roslyn check-coreclr check-ms-test-suite

clean-local: clean-local-coreclr
