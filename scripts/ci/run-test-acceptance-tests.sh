#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

make install  # Roslyn tests need a Mono installation

cd acceptance-tests

LANG=en_US.UTF-8 ${TESTCMD} --label=check-ms-test-suite --timeout=30m make check-ms-test-suite

total_tests=$(find . -name TestResult*xml | xargs cat | grep -c "<test-case")
if [ "$total_tests" -lt "1600" ]
	then echo "*** NOT ENOUGH TEST RESULTS RECORDED, MARKING FAILURE ***"
	exit 1
fi

${TESTCMD} --label=check-roslyn --timeout=30m make check-roslyn PREFIX=${WORKSPACE}/tmp/mono-acceptance-tests
rm -rf ${WORKSPACE}/tmp/mono-acceptance-tests  # cleanup the Mono installation used for Roslyn tests

${TESTCMD} --label=coreclr-compile-tests --timeout=80m --fatal make coreclr-compile-tests
${TESTCMD} --label=coreclr-runtest-basic --timeout=10m make coreclr-runtest-basic
${TESTCMD} --label=coreclr-runtest-coremanglib --timeout=10m make coreclr-runtest-coremanglib
${TESTCMD} --label=coreclr-gcstress --timeout=1200m make coreclr-gcstress
