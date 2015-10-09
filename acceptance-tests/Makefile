TOP=$(abspath ..)
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
	$(MAKE) -C $(TOP)/mono/tests check-coreclr

CLASS=$(TOP)/mcs/class/lib
MONO_WRAPPER=MONO_PATH=$(CLASS)/net_4_x $(TOP)/runtime/mono-wrapper

run-test-ms-test-suite:
	@if $(MAKE) check-ms-test-suite RESET_VERSIONS=1; then \
		$(MAKE) -C $(MSTESTSUITE_PATH)/conformance build MCS="$(MONO_WRAPPER) $(CLASS)/build/mcs.exe -t:library -warn:1 -r:nunit.framework"; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/conformance run NUNIT-CONSOLE="$(MONO_WRAPPER) $(CLASS)/net_4_x/nunit-console.exe -nologo -exclude=MonoBug,BadTest"; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/systemruntimebringup build MCS="$(MONO_WRAPPER) $(CLASS)/build/mcs.exe -debug -warn:1"; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/systemruntimebringup run MONO="$(MONO_WRAPPER)"; \
	else \
		echo "*** [ms-test-suite] Getting the repository failed, you probably don't have access to this Xamarin-internal resource. Skipping."; \
	fi

EXTRA_DISTFILES=README.md SUBMODULES.json versions.mk versions.rb
