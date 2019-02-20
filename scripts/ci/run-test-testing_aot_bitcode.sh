#!/bin/bash -e

# if test -n "${MONO_STATIC_AOT}";
# then
# ${TESTCMD} --label=mkbundle --timeout=25m make -j ${CI_CPU_COUNT} -w -C mcs/class/corlib -k mkbundle-all-tests
# fi

# ${TESTCMD} --label=mini --timeout=25m make -j ${CI_CPU_COUNT} -w -C mono/mini -k llvmonlycheck

# ${TESTCMD} --label=runtime --timeout=160m make -w -C mono/tests -k test-wrench V=1
# ${TESTCMD} --label=corlib --timeout=30m make -w -C mcs/class/corlib run-test

rm -fr /tmp/jenkins-temp-aspnet*

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
