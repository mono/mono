#!/bin/bash -e

source ${MONO_REPO_ROOT}/scripts/ci/util.sh

helix_send_build_start_event "build/tests/$MONO_HELIX_TYPE/"
${TESTCMD} --label=compile-runtime-tests --timeout=20m --fatal make -w -C mono -j ${CI_CPU_COUNT} test
# workaround some races in the tests build
for dir in mcs/tools/nunit-lite mcs/class/Microsoft.Build*; do
    ${TESTCMD} --label=compile-$(basename $dir) --timeout=5m make -w -C $dir test xunit-test
    ${TESTCMD} --label=compile-$(basename $dir)-xbuild_12 --timeout=5m make -w -C $dir test xunit-test PROFILE=xbuild_12
    ${TESTCMD} --label=compile-$(basename $dir)-xbuild_14 --timeout=5m make -w -C $dir test xunit-test PROFILE=xbuild_14
done
${TESTCMD} --label=compile-bcl-tests --timeout=40m --fatal make -w -C runtime -j ${CI_CPU_COUNT} test xunit-test
${TESTCMD} --label=create-test-payload --timeout=5m --fatal make -w test-bundle TEST_BUNDLE_PATH="$MONO_REPO_ROOT/mono-test-bundle"
helix_send_build_done_event "build/tests/$MONO_HELIX_TYPE/" 0

# get libgdiplus from CI output
if [[ ${CI_TAGS} == *'linux-amd64'* ]]; then
    wget "https://xamjenkinsartifact.blob.core.windows.net/test-libgdiplus-mainline/280/debian-9-amd64/src/.libs/libgdiplus.so" -O "$MONO_REPO_ROOT/mono-test-bundle/mono-libgdiplus.so"
else
    echo "Unknown OS, couldn't determine appropriate libgdiplus."
    exit 1
fi

export MONO_HELIX_TEST_PAYLOAD_DIRECTORY="$MONO_REPO_ROOT/mono-test-bundle"
export MONO_HELIX_CORRELATION_ID_FILE="$MONO_REPO_ROOT/test-correlation-id.txt"
${TESTCMD} --label=upload-helix-tests --timeout=5m --fatal make -w -C mcs/tools/mono-helix-client upload-to-helix

# test suites which aren't ported to helix yet
${TESTCMD} --label=mini-aotcheck --timeout=5m make -j ${CI_CPU_COUNT} -w -C mono/mini -k aotcheck
${TESTCMD} --label=runtime --timeout=20m make -w -C mono/tests test-wrench IGNORE_TEST_JIT=1 V=1
${TESTCMD} --label=runtime-unit-tests --timeout=5m make -w -C mono/unit-tests -k check
${TESTCMD} --label=runtime-eglib-tests --timeout=5m make -w -C mono/eglib/test -k check
${TESTCMD} --label=monolinker --timeout=10m make -w -C mcs/tools/linker check
${TESTCMD} --label=System.Web.Extensions-standalone --timeout=5m make -w -C mcs/class/System.Web.Extensions run-standalone-test

# wait for helix tests to complete
export MONO_HELIX_CORRELATION_ID=$(cat "$MONO_HELIX_CORRELATION_ID_FILE")
${TESTCMD} --label=wait-helix-tests --timeout=40m make -w -C mcs/tools/mono-helix-client wait-for-job-completion

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
