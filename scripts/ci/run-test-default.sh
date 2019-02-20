#!/bin/bash -e

source ${MONO_REPO_ROOT}/scripts/ci/util.sh

${TESTCMD} --label=mini --timeout=5m make -w -C mono/mini -k check check-seq-points EMIT_NUNIT=1
if [[ ${CI_TAGS} == *'win-'* ]]
then ${TESTCMD} --label=mini-aotcheck --skip;
else ${TESTCMD} --label=mini-aotcheck --timeout=5m make -j ${CI_CPU_COUNT} -w -C mono/mini -k aotcheck
fi
if [[ ${CI_TAGS} == *'win-'* ]] || [[ ${CI_TAGS} == *'ppc64'* ]]
then ${TESTCMD} --label=aot-test --skip;
else ${TESTCMD} --label=aot-test --timeout=30m make -w -C mono/tests -j ${CI_CPU_COUNT} -k test-aot
fi
${TESTCMD} --label=compile-bcl-tests --timeout=40m make -i -w -C runtime -j ${CI_CPU_COUNT} test xunit-test
${TESTCMD} --label=compile-runtime-tests --timeout=40m make -w -C mono/tests -j ${CI_CPU_COUNT} test
if [[ ${CI_TAGS} == *'osx-'* ]]; then ${TESTCMD} --label=llvmonly-mixed --timeout=10m make -w -C mono/tests/llvmonly-mixed -j ${CI_CPU_COUNT} check; fi
if [[ ${CI_TAGS} == *'osx-'* ]]; then ${TESTCMD} --label=corlib-btls --timeout=5m bash -c "export MONO_TLS_PROVIDER=btls && make -w -C mcs/class/corlib TEST_HARNESS_FLAGS=-include:X509Certificates run-test"; fi
${TESTCMD} --label=System-xunit --timeout=5m make -w -C mcs/class/System run-xunit-test
if [[ ${CI_TAGS} == *'osx-'* ]]; then ${TESTCMD} --label=System-btls --timeout=10m bash -c "export MONO_TLS_PROVIDER=btls && make -w -C mcs/class/System run-test"; fi

${TESTCMD} --label=bundle-test-results --timeout=2m find . -name "TestResult*.xml" -exec tar -rvf TestResults.tar {} \;

rm -fr /tmp/jenkins-temp-aspnet*

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
