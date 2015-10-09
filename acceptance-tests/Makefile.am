TOP=..
ROSLYN_PATH=$(TOP)/../roslyn
CORECLR_PATH=$(TOP)/../coreclr
MSTESTSUITE_PATH=$(TOP)/../ms-test-suite

include versions.mk

check: run-test

run-test: run-test-roslyn run-test-coreclr run-test-ms-test-suite

run-test-roslyn:
	@$(MAKE) check-roslyn RESET_VERSIONS=1
	echo Test

run-test-coreclr:
	@$(MAKE) check-coreclr RESET_VERSIONS=1
	$(MAKE) -C $(top_srcdir)/mono/tests check-coreclr

CLASS=$(mcs_topdir)/class/lib/$(DEFAULT_PROFILE)

with_mono_path = MONO_PATH=$(CLASS)
RUNTIME = $(with_mono_path) $(abs_top_builddir)/runtime/mono-wrapper
MCS = $(RUNTIME) $(mcs_topdir)/class/lib/build/mcs.exe

run-test-ms-test-suite:
	@if $(MAKE) check-ms-test-suite RESET_VERSIONS=1; then \
		$(MAKE) -C $(MSTESTSUITE_PATH)/conformance build MCS="$(MCS) -t:library -warn:1 -r:nunit.framework"; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/conformance run NUNIT-CONSOLE="$(RUNTIME) $(CLASS)/nunit-console.exe -nologo -exclude=MonoBug,BadTest"; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/systemruntimebringup build MCS="$(MCS) -debug -warn:1"; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/systemruntimebringup run MONO="$(RUNTIME)"; \
	else \
		echo "*** [ms-test-suite] Getting the repository failed, you probably don't have access to this Xamarin-internal resource. Skipping."; \
	fi

EXTRA_DISTFILES=README.md SUBMODULES.json versions.mk versions.rb
