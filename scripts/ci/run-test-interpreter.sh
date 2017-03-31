#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=interpreter-regression --timeout=10m make -C mono/mini richeck
${TESTCMD} --label=mixedmode-regression --timeout=10m make -C mono/mini mixedcheck
${TESTCMD} --label=compile-runtime-tests --timeout=40m make -w -C mono/tests -j4 tests
${TESTCMD} --label=runtime-interp --timeout=160m make -w -C mono/tests -k testinterp V=1
