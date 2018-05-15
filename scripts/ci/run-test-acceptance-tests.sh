#!/bin/bash -e

#
# The timeouts are double of time the execution usually takes
#

# run the MS test suite
LANG=en_US.UTF-8 ${TESTCMD} --label=check-ms-test-suite --timeout=10m make -C acceptance-tests check-ms-test-suite

total_tests=$(find acceptance-tests/ -name TestResult*xml | xargs cat | grep -c "<test-case")
if [ "$total_tests" -lt "1600" ]
	then echo "*** NOT ENOUGH TEST RESULTS RECORDED, MARKING FAILURE ***"
	exit 1
fi

# run Roslyn tests
${TESTCMD} --label=check-roslyn --timeout=60m make -C acceptance-tests check-roslyn

# run CoreCLR managed tests, we precompile them in parallel so individual steps don't need to do it
${TESTCMD} --label=coreclr-compile-tests --timeout=140m --fatal make -C acceptance-tests coreclr-compile-tests
${TESTCMD} --label=coreclr-runtest-basic --timeout=20m make -C acceptance-tests coreclr-runtest-basic
${TESTCMD} --label=coreclr-runtest-coremanglib --timeout=10m make -C acceptance-tests coreclr-runtest-coremanglib

# run the GC stress tests (on PRs we only run a short version)
${TESTCMD} --label=coreclr-gcstress --timeout=1200m make -C acceptance-tests coreclr-gcstress CI_PR=$([[ ${CI_TAGS} == *'pull-request'* ]] && echo 1 || true)
