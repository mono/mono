#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh
export TEST_WITH_INTERPRETER=1

${TESTCMD} --label=interpreter-regression --timeout=10m make -C mono/mini richeck
${TESTCMD} --label=mixedmode-regression --timeout=10m make -C mono/mini mixedcheck
${TESTCMD} --label=compile-runtime-tests --timeout=40m make -w -C mono/tests -j4 tests
${TESTCMD} --label=runtime-interp --timeout=160m make -w -C mono/tests -k testinterp V=1 CI=1 CI_PR=${ghprbPullId}
${TESTCMD} --label=corlib --timeout=160m make -w -C mcs/class/corlib run-test V=1
if [[ ${label} != 'debian-8-armhf' ]]; then ${TESTCMD} --label=mcs-tests --timeout=160m make -w -C mcs/tests run-test V=1; fi
${TESTCMD} --label=Mono.Debugger.Soft --timeout=5m make -w -C mcs/class/Mono.Debugger.Soft run-test V=1
