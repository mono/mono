#!/bin/bash -e

#
# The timeouts are double of time the execution usually takes
#

# run the MS test suite
if [[ ${CI_TAGS} == *'ms-test-suite'* ]]; then
	LANG=en_US.UTF-8 ${TESTCMD} --label=check-ms-test-suite --timeout=10m make -C acceptance-tests check-ms-test-suite

	total_tests=$(find acceptance-tests/ -name TestResult*xml | xargs cat | grep -c "<test-case")
	if [ "$total_tests" -lt "1600" ]
		then echo "*** NOT ENOUGH TEST RESULTS RECORDED, MARKING FAILURE ***"
		exit 1
	fi
fi

# run Roslyn tests, they use Mono from PATH so we need to do a temporary install
${TESTCMD} --label=install-temp-mono --timeout=10m make install
OLD_PATH=$PATH
export PATH=${MONO_REPO_ROOT}/tmp/mono-acceptance-tests/bin:$PATH
${TESTCMD} --label=check-roslyn --timeout=60m make -C acceptance-tests check-roslyn
export PATH=$OLD_PATH
rm -rf "${MONO_REPO_ROOT}/tmp/mono-acceptance-tests"

# run CoreCLR managed tests, we precompile them in parallel so individual steps don't need to do it
${TESTCMD} --label=coreclr-compile-tests --timeout=140m --fatal make -C acceptance-tests coreclr-compile-tests
${TESTCMD} --label=coreclr-runtest-basic --timeout=20m make -C acceptance-tests coreclr-runtest-basic
${TESTCMD} --label=coreclr-runtest-coremanglib --timeout=10m make -C acceptance-tests coreclr-runtest-coremanglib

# run the GC stress tests (on PRs we only run a short version)
${TESTCMD} --label=coreclr-gcstress --timeout=1200m make -C acceptance-tests coreclr-gcstress

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
