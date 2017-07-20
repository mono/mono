#!/bin/bash -e

make install  # Roslyn tests need a Mono installation

LANG=en_US.UTF-8 ${TESTCMD} --label=check-ms-test-suite --timeout=30m make -C acceptance-tests check-ms-test-suite

total_tests=$(find acceptance-tests/ -name TestResult*xml | xargs cat | grep -c "<test-case")
if [ "$total_tests" -lt "1600" ]
	then echo "*** NOT ENOUGH TEST RESULTS RECORDED, MARKING FAILURE ***"
	exit 1
fi

${TESTCMD} --label=check-roslyn --timeout=30m make -C acceptance-tests check-roslyn PREFIX=${MONO_REPO_ROOT}/tmp/mono-acceptance-tests
rm -rf ${MONO_REPO_ROOT}/tmp/mono-acceptance-tests  # cleanup the Mono installation used for Roslyn tests

${TESTCMD} --label=coreclr-compile-tests --timeout=80m --fatal make -C acceptance-tests coreclr-compile-tests
${TESTCMD} --label=coreclr-runtest-basic --timeout=10m make -C acceptance-tests coreclr-runtest-basic
${TESTCMD} --label=coreclr-runtest-coremanglib --timeout=10m make -C acceptance-tests coreclr-runtest-coremanglib
${TESTCMD} --label=coreclr-gcstress --timeout=1200m make -C acceptance-tests coreclr-gcstress
