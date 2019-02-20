#!/bin/bash -e

if test -n "${MONO_LLVMONLY}";
then
${TESTCMD} --label=runtime --timeout=160m make -w -C mono/tests -k testllvmonlyinterp V=1
else
${TESTCMD} --label=runtime --timeout=160m make -w -C mono/tests -k testfullaotinterp V=1
fi

${TESTCMD} --label=System --timeout=10m make -w -C mcs/class/System run-test

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
