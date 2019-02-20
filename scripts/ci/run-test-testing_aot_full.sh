#!/bin/bash -e

if test -n "${MONO_STATIC_AOT}";
then
${TESTCMD} --label=mkbundle --timeout=25m make -j ${CI_CPU_COUNT} -w -C mcs/class/corlib -k mkbundle-all-tests
fi

if test -n "${MONO_LLVMONLY}";
then
${TESTCMD} --label=mini --timeout=25m make -j ${CI_CPU_COUNT} -w -C mono/mini -k llvmonlycheck
else
${TESTCMD} --label=mini --timeout=25m make -j ${CI_CPU_COUNT} -w -C mono/mini -k fullaotcheck
fi

${TESTCMD} --label=System --timeout=10m make -w -C mcs/class/System run-test

rm -fr /tmp/jenkins-temp-aspnet*

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
