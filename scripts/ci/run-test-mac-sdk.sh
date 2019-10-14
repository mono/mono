#!/bin/bash -e
# -*- mode: shell-script; indent-tabs-mode: nil; -*-

${TESTCMD} --label=bockbuild --timeout=300m --fatal ${MONO_REPO_ROOT}/scripts/mac-sdk-package.sh

# switch to using package Mono instead of system
export PATH=${MONO_REPO_ROOT}/external/bockbuild/stage/bin:$PATH

# Bundled MSBuild
cd ${MONO_REPO_ROOT}/external/bockbuild/builds/msbuild-15/
${TESTCMD} --label="msbuild-tests" --timeout=180m ./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release
zip ${MONO_REPO_ROOT}/msbuild-test-results.zip artifacts/TestResults/Release-MONO/* artifacts/log/Release-MONO/*.log

if [[ $CI_TAGS == *'msbuild-tests-only'* ]]
then echo "Running only msbuild tests"
else
    # Bundled LLVM
    cd ${MONO_REPO_ROOT}/external/bockbuild/builds/mono
    ${TESTCMD} --label="compile-runtime-tests" --timeout=240m make -j ${CI_CPU_COUNT} -C mono/tests -k test V=1 M=1
    ${TESTCMD} --label="runtime-tests-llvm" --timeout=240m make -j ${CI_CPU_COUNT} -C mono/tests -k test-wrench MONO_ENV_OPTIONS=--llvm V=1 M=1
    ${TESTCMD} --label="corlib-tests-llvm" --timeout=60m make -j ${CI_CPU_COUNT} -C mcs/class/corlib run-test PLATFORM_TEST_HARNESS_EXCLUDES="NotOnMac LLVMNotWorking" MONO_ENV_OPTIONS=--llvm

    # Bundled libgdiplus
    ${TESTCMD} --label="System.Drawing" --timeout=60m make -C mcs/class/System.Drawing run-test
fi

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
