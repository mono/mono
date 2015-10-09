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
	(cd $(TOP)/mono/tests && $(MAKE) check-coreclr)

run-test-ms-test-suite:
	@if $(MAKE) check-ms-test-suite RESET_VERSIONS=1; then \
		echo Test; \
	else \
		echo "*** [ms-test-suite] Getting the repository failed, you probably don't have access to this Xamarin-internal resource. Skipping."; \
	fi

EXTRA_DISTFILES=README.md SUBMODULES.json versions.mk versions.rb
