TOP=..
ROSLYN_PATH=$(TOP)/../roslyn
CORECLR_PATH=$(TOP)/../coreclr
MSTESTSUITE_PATH=$(TOP)/../ms-test-suite

include versions.mk

check: run-test

run-test: run-test-roslyn run-test-coreclr run-test-ms-test-suite

run-test-roslyn:
	echo Test

run-test-coreclr:
	(cd $(TOP)/mono/tests && $(MAKE) check-coreclr)

run-test-ms-test-suite:
	echo Test

EXTRA_DISTFILES=README.md SUBMODULES.json versions.mk versions.rb
