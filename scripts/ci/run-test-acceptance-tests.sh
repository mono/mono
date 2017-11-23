#!/bin/bash -e

LANG=en_US.UTF-8 ${TESTCMD} --label=check-ms-test-suite --timeout=10m make -C acceptance-tests check-ms-test-suite

total_tests=$(find acceptance-tests/ -name TestResult*xml | xargs cat | grep -c "<test-case")
if [ "$total_tests" -lt "1600" ]
	then echo "*** NOT ENOUGH TEST RESULTS RECORDED, MARKING FAILURE ***"
	exit 1
fi

${TESTCMD} --label=check-roslyn --timeout=60m make -C acceptance-tests check-roslyn

#${TESTCMD} --label=coreclr-compile-tests --timeout=80m --fatal make -C acceptance-tests coreclr-compile-tests
${TESTCMD} --label=coreclr-runtest-basic --timeout=45m make -C acceptance-tests coreclr-runtest-basic
${TESTCMD} --label=coreclr-runtest-coremanglib --timeout=90m make -C acceptance-tests coreclr-runtest-coremanglib
#${TESTCMD} --label=coreclr-gcstress --timeout=1200m make -C acceptance-tests coreclr-gcstress
